using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Intuit.Ipp.Data;
using Intuit.Ipp.Exception;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.WebhooksService;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;
using Report = QuickBooks.Models.DAL.Report;
using TaxRate = Intuit.Ipp.Data.TaxRate;


namespace QuickBooks.Models.Business
{
    public abstract class BaseService<T> : IBaseService<T> where T : SalesTransaction
    {
        protected BaseService(IReportRepository reportRepository, ITaxRepository taxRepository,
            IOAuthService oAuthService, string entityName)
        {
            _reportRepository = reportRepository;
            _log = LogManager.GetLogger(GetType());
            EntityName = entityName;
            _taxRepository = taxRepository;
            _oAuthService = oAuthService;
        }

        private readonly ILog _log;
        private readonly IReportRepository _reportRepository;
        private readonly ITaxRepository _taxRepository;
        public string EntityName { get; }
        private IDictionary<string, decimal> _taxRateDictionary;
        protected readonly IOAuthService _oAuthService;

        public virtual IList<T> Calculate(string realmId,IList<T> list = null)
        {
            var entityId = "";
            try
            {
                _taxRateDictionary = GetCustomersTaxRate();
                var dataService = _oAuthService.GetDataService(realmId);
                var queryService = _oAuthService.GetQueryService<T>(realmId);
                var entities = list ?? queryService.Select(x => x).ToList();
                //                var entities = queryService.Select(x => x).Take(1).ToList();
                if (entities.Count == 0) return new List<T>();
                foreach (var entity in entities)
                {
                    entityId = entity.Id;
                    SetTaxCode(entity);
                    var countrySubDivisionCode = entity.ShipAddr?.CountrySubDivisionCode;
                    if (countrySubDivisionCode == null) continue;
                    var percent = GetPercent(countrySubDivisionCode);
                    SetTaxCodeRef(realmId, percent, entity);
                    if (entity.TxnTaxDetail.TotalTax == 0) RecalculateTaxManually(entity, percent);
                    dataService.Update(entity);
                }
                return entities;
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

        public virtual void Save(string realmId, IList<T> list = null)
        {
            var entityId = "";
            try
            {
                var queryService = _oAuthService.GetQueryService<T>(realmId);
                var entities = list ?? queryService.Select(x => x).ToList();
                foreach (var entity in entities)
                {
                    var document = new Report {Id = entity.Id};
                    entityId = entity.Id;
                    document.DocumentNumber = entity.DocNumber;
                    document.SaleDate = entity.TxnDate;
                    if (entity.BillAddr.Line1 != null) document.CustomerName = entity.CustomerRef.name;
                    var address = new StringBuilder();
                    if (entity.ShipAddr != null)
                    {
                        if (entity.ShipAddr.Line1 != null) address.Append(entity.ShipAddr.Line1);
                        if (entity.ShipAddr.City != null) address.Append(" " + entity.ShipAddr.City);
                        if (entity.ShipAddr.CountrySubDivisionCode != null)
                            address.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                        if (entity.ShipAddr.PostalCode != null) address.Append(", " + entity.ShipAddr.PostalCode);
                    }
                    document.ShipToAddress = address.ToString();
                    if (entity.Line == null)
                    {
                        _reportRepository.Save(document);
                        continue;
                    }
                    document.LineItems = GetLineItems(entity);
                    _reportRepository.Save(document);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to save {EntityName} with id = {entityId}", e);
                throw;
            }
        }

        private static List<LineItem> GetLineItems(T entity)
        {
            var lineItems = new List<LineItem>();
            foreach (var line in entity.Line)
            {
                var lineItem = new LineItem();
                if (line == null) continue;
                lineItem.Amount = line.Amount;
                if (!(line.AnyIntuitObject is SalesItemLineDetail)) continue;
                lineItem.Quantity = (int) ((SalesItemLineDetail) line.AnyIntuitObject).Qty;
                if (((SalesItemLineDetail) line.AnyIntuitObject).ItemRef == null) continue;
                lineItem.Name = ((SalesItemLineDetail) line.AnyIntuitObject).ItemRef.name;
                lineItems.Add(lineItem);
            }
            return lineItems;
        }

       
        public void Process(string realmId, Entity entity)
        {
            var entityId = entity.Id;
            try
            {
                if (entity.Operation == "Delete")
                {
                    _reportRepository.Delete(entity.Id);
                    return;
                }
                var service = _oAuthService.GetQueryService<T>(realmId);
                var entityFromQuickBooks = service.Where(x => x.Id == entity.Id).ToList();
                if (entityFromQuickBooks.Count == 0) return;
                if (entity.Operation != "Create")
                {
                    if (entity.Operation != "Update") return;
                    var reportEntity = _reportRepository.Get(entity.Id);
                    if (reportEntity == null)
                    {
                        Calculate(realmId,entityFromQuickBooks);
                        return;
                    }
                    if (IsEqualLines(entityFromQuickBooks[0].Line, reportEntity.LineItems)) return;
                    var recalculatedList = Calculate(realmId,entityFromQuickBooks);
                    _reportRepository.Delete(entity.Id);
                    Save(realmId,recalculatedList);
                }
                else
                {
                    var recalculatedList = Calculate(realmId,entityFromQuickBooks);
                    Save(realmId,recalculatedList);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to update data with id = {entityId}", e);
                throw;
            }
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
            var taxCodeId = GetTaxRateId(realmId,taxRate);
            if (taxCodeId != null)
            {
                var taxCodeQueryService = _oAuthService.GetQueryService<TaxCode>(realmId);
                var stateTaxCodes = taxCodeQueryService.ExecuteIdsQuery("Select * From TaxCode");
                var taxCode = stateTaxCodes.Where(code => code != null)
                    .Where(code => code.SalesTaxRateList != null)
                    .Where(code => code.SalesTaxRateList.TaxRateDetail != null)
                    .Where(code => code.SalesTaxRateList.TaxRateDetail[0] != null)
                    .Where(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef != null)
                    .Where(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef.Value != null)
                    .FirstOrDefault(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef.Value == taxCodeId);
                if (taxCode != null)
                {
                    var codeRefValue = taxCode.Id;
                    return codeRefValue;
                }
            }
            var taxService = AddTaxService(realmId,taxRate);
            return taxService.TaxRateDetails[0].TaxRateId;
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
            if (string.IsNullOrEmpty(countrySubDivisionCode)) return _taxRateDictionary["DEFAULT"];
            if (_taxRateDictionary.ContainsKey(countrySubDivisionCode.ToUpper()))
            {
                taxRate = _taxRateDictionary[countrySubDivisionCode.ToUpper()];
                return taxRate;
            }
            taxRate = _taxRateDictionary["DEFAULT"];
            return taxRate;
        }

        private IDictionary<string, decimal> GetCustomersTaxRate()
        {
            var taxRateList = _taxRepository.List();
            return taxRateList.ToDictionary(item => item.CountrySubDivisionCode.ToUpper(), item => item.Tax);
        }

        private string GetTaxRateId(string realmId, decimal taxRate)
        {
            var queryService = _oAuthService.GetQueryService<TaxRate>(realmId);
            var taxRateId =
                queryService.Where(x => x.RateValue == taxRate && x.Active).Select(x => x.Id).FirstOrDefault();
            return taxRateId;
        }

        public TaxService AddTaxService(string realmId, decimal percent)
        {
            var queryService = _oAuthService.GetQueryService<TaxAgency>(realmId);
            var taxAgency = queryService.Where(x => x != null).FirstOrDefault();
            if (taxAgency == null)
            {
                var dataService = _oAuthService.GetDataService(realmId);
                taxAgency = dataService.Add(new TaxAgency {DisplayName = "New Tax Agency"});
            }
            var name = $"{percent} percent tax";
            var taxRateDetailses = new[]
            {
                new TaxRateDetails
                {
                    RateValue = percent,
                    RateValueSpecified = true,
                    TaxAgencyId = taxAgency.Id,
                    TaxApplicableOn = TaxRateApplicableOnEnum.Sales,
                    TaxRateName = name
                }
            };
            var taxService = new TaxService {TaxCode = name, TaxRateDetails = taxRateDetailses};
            var globalTaxService = _oAuthService.GetGlobalTaxService(realmId);
            taxService = globalTaxService.AddTaxCode(taxService);
            return taxService;
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
            var taxRateRef = GetTxnCodeRefValue(realmId,percent);
            entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType {Value = taxRateRef};
        }

        IList IService.Calculate(string realmId, IList list)
        {
            if (list == null) return (IList) Calculate(realmId);
            IList<T> genericList = list.Cast<T>().ToList();
            return (IList) Calculate(realmId,genericList);
        }

        void IPersisting.Save(string realmId, IList list)
        {
            if (list != null)
            {
                IList<T> genericList = list.Cast<T>().ToList();
                Save(realmId,genericList);
            }
            Save(realmId);
        }
    }
}