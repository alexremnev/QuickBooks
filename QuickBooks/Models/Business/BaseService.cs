using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Intuit.Ipp.Data;
using Intuit.Ipp.Exception;
using Intuit.Ipp.WebhooksService;
using QuickBooks.Models.Data;
using QuickBooks.Models.Repository;
using static System.Decimal;
using Report = QuickBooks.Models.Data.Report;


namespace QuickBooks.Models.Business
{
    public abstract class BaseService<T> : IBaseService where T : SalesTransaction
    {
        protected BaseService(IReportRepository reportRepository, ITaxRateProvider taxRateProvider,
            IOAuthService oAuthService, IQBApi qb, string entityName)
        {
            _reportRepository = reportRepository;
            _log = LogManager.GetLogger(GetType());
            EntityName = entityName;
            _taxRateProvider = taxRateProvider;
            _oAuthService = oAuthService;
            _qb = qb;
        }

        private readonly ILog _log;
        private readonly IReportRepository _reportRepository;
        private readonly ITaxRateProvider _taxRateProvider;
        public string EntityName { get; }
        protected readonly IOAuthService _oAuthService;
        protected readonly IQBApi _qb;

        public virtual IList<T> Calculate(string realmId)
        {
            var list = _qb.List<T>(realmId);
            return list.Length != 0 ? Calculate(realmId, list) : new List<T>();
        }

        protected virtual IList<T> Calculate(string realmId, params T[] list)
        {
            var entityId = "";
            try
            {
                foreach (var entity in list)
                {
                    entityId = entity.Id;
                    SetTaxCode(entity);
                    var countrySubDivisionCode = entity.ShipAddr?.CountrySubDivisionCode;
                    if (countrySubDivisionCode == null) continue;
                    var percent = GetPercent(countrySubDivisionCode);
                    SetTaxCodeRef(realmId, percent, entity);
                    if (entity.TxnTaxDetail.TotalTax == 0) RecalculateTaxManually(entity, percent);
                    _qb.Update(realmId, entity);
                }
                return list;
            }
            catch (ValidationException e)
            {
                _log.Error(
                    $"Exception occured when application tried to recalculate sales tax in {EntityName} with id = {entityId}",
                    e);
                throw;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("An error occured while executing the query."))
                    _log.Error("In Quickbooks Online something went wrong. It can't execute a simple query!!!");
                _log.Error(
                    $"Exception occured when application tried to recalculate sales tax in {EntityName} with id = {entityId}",
                    e);
                throw;
            }
        }

        public virtual void Save(string realmId)
        {
            var list = _qb.List<T>(realmId);
            Save(realmId, list);
        }

        protected virtual void Save(string realmId, IList<T> list)
        {
            var entityId = "";
            try
            {
                foreach (var entity in list)
                {
                    entityId = entity.Id;
                    var document = ExtractDocument(realmId, entity);
                    _reportRepository.Save(document);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to save {EntityName} with id = {entityId}", e);
                throw;
            }
        }

        public void Update(string realmId, Entity entity)
        {
            var entityId = entity.Id;
            try
            {
                switch (entity.Operation)
                {
                    case "Delete":
                        DeleteDocument(realmId, entity);
                        return;
                    case "Create":
                        CreateDocument(realmId, entity);
                        return;
                    case "Update":
                        UpdateDocument(realmId, entity);
                        return;
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to update data with id = {entityId}", e);
                throw;
            }
        }

        private void DeleteDocument(string realmId, Entity entity)
        {
            _reportRepository.Delete(realmId + entity.Id);
        }

        private void UpdateDocument(string realmId, Entity entity)
        {
            var entityFromQuickBooks = _qb.GetById<T>(realmId, entity.Id);
            if (entityFromQuickBooks == null) return;
            var reportEntity = _reportRepository.Get(realmId + entity.Id);
            if (reportEntity == null)
            {
                var recalculatedEntity = Calculate(realmId, entityFromQuickBooks);
                Save(realmId, recalculatedEntity);
                return;
            }
            if (IsEqualLines(entityFromQuickBooks.Line, reportEntity.LineItems)) return;
            var recalculatedList = Calculate(realmId, entityFromQuickBooks);
            DeleteDocument(realmId, entity);
            Save(realmId, recalculatedList);
        }

        private void CreateDocument(string realmId, Entity entity)
        {
            var entityFromQuickBooks = _qb.GetById<T>(realmId, entity.Id);
            if (entityFromQuickBooks == null) return;
            var recalculatedList = Calculate(realmId, entityFromQuickBooks);
            Save(realmId, recalculatedList);
        }

       private static Report ExtractDocument(string realmId, T entity)
        {
            var document = new Report
            {
                Id = realmId + entity.Id,
                DocumentNumber = entity.DocNumber,
                SaleDate = entity.TxnDate,
                CustomerName = entity.CustomerRef.name,
                ShipToAddress = entity.ShipAddr,
                LineItems = ExtractLineItems(entity)
            };
            return document;
        }

        private static List<LineItem> ExtractLineItems(T entity)
        {
            var lineItems = new List<LineItem>();
            lineItems.AddRange(entity.Line
                .Where(line => line != null)
                .Where(line => line.AnyIntuitObject is SalesItemLineDetail)
                .Select(line => new LineItem
                {
                    Amount = line.Amount,
                    Quantity = ToInt32(((SalesItemLineDetail) line.AnyIntuitObject).Qty),
                    Name = ((SalesItemLineDetail) line.AnyIntuitObject).ItemRef.name
                }));

            return lineItems;
        }

        private static bool IsEqualLines(IList<Line> quickBookslines, IList<LineItem> actualLines)
        {
            if (quickBookslines.Count - actualLines.Count != 0) return false;
            for (var i = 0; i < quickBookslines.Count; i++)
            {
                if (quickBookslines[i].DetailType == LineDetailTypeEnum.SubTotalLineDetail) return false;
                if (quickBookslines[i].Amount != actualLines[i].Amount) return false;
            }
            return true;
        }

        private string GetTxnCodeRefValue(string realmId, decimal taxRate)
        {
            var taxRateId = _qb.GetTaxRateId(realmId, taxRate);
            if (taxRateId == null) return _qb.GetNewTaxCodeId(realmId, taxRate);
            var taxCode = _qb.GetTaxCode(realmId, taxRateId);
            return taxCode != null ? taxCode.Id : _qb.GetNewTaxCodeId(realmId, taxRate);
        }

        private static void SetTaxCode(T entity)
        {
            var isNeedToAddLine = true;
            foreach (var line in entity.Line)
            {
                var lineDetail = line.AnyIntuitObject as SalesItemLineDetail;
                if (lineDetail == null) continue;
                if (isNeedToAddLine) isNeedToAddLine = false;
                if (lineDetail.TaxCodeRef == null) lineDetail.TaxCodeRef = new ReferenceType {Value = "TAX"};
                lineDetail.TaxCodeRef.Value = "TAX";
            }
            if (isNeedToAddLine) AddLine(entity);
        }

        private decimal GetPercent(string countrySubDivisionCode)
        {
            decimal taxRate;
            var _taxRateDictionary = _taxRateProvider.GetTaxRates();
            if (string.IsNullOrEmpty(countrySubDivisionCode)) return _taxRateDictionary["DEFAULT"];
            if (_taxRateDictionary.ContainsKey(countrySubDivisionCode.ToUpper()))
            {
                taxRate = _taxRateDictionary[countrySubDivisionCode.ToUpper()];
                return taxRate;
            }
            taxRate = _taxRateDictionary["DEFAULT"];
            return taxRate;
        }

        private static void RecalculateTaxManually(Transaction entity, decimal percent)
        {
            decimal totalTax = 0;
            foreach (var line in entity.Line)
            {
                var lineDetail = line.AnyIntuitObject as SalesItemLineDetail;
                if (lineDetail == null) continue;
                totalTax += line.Amount*percent/100;
            }
            entity.TxnTaxDetail.TotalTax = totalTax;
        }

        private static void AddLine(Transaction entity)
        {
            foreach (var line in entity.Line)
            {
                var subTotalLineItem = line.AnyIntuitObject as SubTotalLineDetail;
                if (subTotalLineItem == null) continue;
                var newLine = new Line
                {
                    Amount = line.Amount,
                    AmountSpecified = true,
                    DetailTypeSpecified = true,
                    DetailType = LineDetailTypeEnum.SalesItemLineDetail,
                    LineNum = 1.ToString(),
                    AnyIntuitObject =
                        new SalesItemLineDetail
                        {
                            ItemRef = new ReferenceType {Value = 1.ToString()},
                            TaxCodeRef = new ReferenceType {Value = "Tax"}
                        }
                };
                var newLines = new[] {newLine, line};
                entity.Line = newLines;
                return;
            }
        }

        private void SetTaxCodeRef(string realmId, decimal percent, Transaction entity)
        {
            var taxRateRef = GetTxnCodeRefValue(realmId, percent);
            entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType {Value = taxRateRef};
        }

        IList ICalculatingService.Calculate(string realmId)
        {
            return (IList) Calculate(realmId);
        }

        void IPersistingService.Save(string realmId)
        {
            Save(realmId);
        }
    }
}