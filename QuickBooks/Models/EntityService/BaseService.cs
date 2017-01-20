using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.GlobalTaxService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using QuickBooks.Models.DAL;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;
using TaxRate = Intuit.Ipp.Data.TaxRate;

namespace QuickBooks.Models.EntityService
{
    public abstract class BaseService<T> : IBaseService<T> where T : SalesTransaction
    {
        private readonly ILog _log;
        private readonly BaseEntity _baseEntity;
        protected BaseService(IReportService service, ITaxRepository taxRepository, BaseEntity baseEntity, string entityName)
        {
            _reportservice = service;
            _log = LogManager.GetLogger(GetType());
            _entityName = entityName;
            _taxRepository = taxRepository;
            _baseEntity = baseEntity;
            _taxRateDictionary = GetCustomersTaxRate();
        }

        private readonly IReportService _reportservice;
        private readonly ITaxRepository _taxRepository;
        private readonly string _entityName;
        private readonly IDictionary<string, decimal> _taxRateDictionary;

        public virtual IList<T> Recalculate(ServiceContext context, IList<T> recalculateEntity = null)
        {
            try
            {
                var dataService = new DataService(context);
                var queryService = new QueryService<T>(context);
                var entities = recalculateEntity ?? queryService.Select(x => x).ToList();
                // var entities = queryService.Where(x => x.DocNumber == 1014.ToString()).ToList();
                var customers = GetCustomers(dataService);

                foreach (var entity in entities)
                {
                    SetTaxCode(entity);
                    var countrySubDivisionCode = customers[entity.CustomerRef.name];
                    var percent = GetPercent(countrySubDivisionCode);
                    context.IppConfiguration.Message.Request.SerializationFormat =
                           SerializationFormat.Json;
                    context.IppConfiguration.Message.Response.SerializationFormat =
                        SerializationFormat.Json;
                    var taxRateRef = GetTxnCodeRefValue(context, percent);
                    entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType { Value = taxRateRef };
                    dataService.Update(entity);
                }
                return entities;
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when you tried to recalculate sales tax in {_entityName}", e);
                throw;
            }
        }

        public virtual void Save(IList<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    var lineItems = new List<LineItem>();
                    _baseEntity.Id = entity.Id;
                    _baseEntity.DocumentNumber = entity.DocNumber;
                    _baseEntity.SaleDate = entity.TxnDate;
                    if (entity.BillAddr.Line1 != null) _baseEntity.CustomerName = entity.CustomerRef.name;
                    var address = new StringBuilder();
                    if (entity.ShipAddr != null)
                    {
                        if (entity.ShipAddr.Line1 != null) address.Append(entity.ShipAddr.Line1);
                        if (entity.ShipAddr.City != null) address.Append(" " + entity.ShipAddr.City);
                        if (entity.ShipAddr.CountrySubDivisionCode != null)
                            address.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                        if (entity.ShipAddr.PostalCode != null) address.Append(", " + entity.ShipAddr.PostalCode);
                    }
                    _baseEntity.ShipToAddress = address.ToString();

                    if (entity.Line == null)
                    {
                        _reportservice.Save(_baseEntity);
                        continue;
                    }

                    foreach (var line in entity.Line)
                    {
                        var lineItem = new LineItem();
                        if (line == null) continue;
                        lineItem.Amount = line.Amount;
                        if (!(line.AnyIntuitObject is SalesItemLineDetail)) continue;
                        lineItem.Quantity = (int)((SalesItemLineDetail)line.AnyIntuitObject).Qty;
                        if (((SalesItemLineDetail)line.AnyIntuitObject).ItemRef == null) continue;
                        lineItem.Name = ((SalesItemLineDetail)line.AnyIntuitObject).ItemRef.name;
                        lineItems.Add(lineItem);
                    }
                    _baseEntity.LineItems = lineItems;
                    _reportservice.Save(_baseEntity);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when you tried to save {_entityName}", e);
                throw;
            }
        }

        public void Update(ServiceContext context, NotificationEntity.Entities entity)
        {
            var service = new QueryService<T>(context);
            var entityFromQuickBooks = service.Where(x => x.Id == entity.Id).ToList();
            if (entity.Operation == "Create")
            {
                var recalculatedList = Recalculate(context, entityFromQuickBooks);
                Save(recalculatedList);
            }
            else if (entity.Operation == "Update")
            {
                var reportEntity = _reportservice.Get(entity.Id);
                if (!IsEqualLines(entityFromQuickBooks[0].Line, reportEntity.LineItems))
                {
                    var recalculatedList = Recalculate(context, entityFromQuickBooks);
                    _reportservice.Delete(entity.Id);
                    Save(recalculatedList);
                }
            }
        }

        private static bool IsEqualLines(IList<Line> quickBookslines, IList<LineItem> actuaLines)
        {
            if (quickBookslines.Count - actuaLines.Count != 0) return false;
            for (var i = 0; i < quickBookslines.Count; i++)
            {
                if (quickBookslines[i].DetailType == LineDetailTypeEnum.SubTotalLineDetail) return false;
                if (quickBookslines[i].Amount != actuaLines[i].Amount) return false;
            }
            return true;
        }

        private string GetTxnCodeRefValue(ServiceContext context, decimal taxRate)
        {
            var taxCodeId = GetTaxRateId(context, taxRate);
            if (taxCodeId != null)
            {
                var taxCodeQueryService = new QueryService<TaxCode>(context);
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
            var taxService = AddTaxService(context, taxRate);
            return taxService.TaxRateDetails[0].TaxRateId;
        }

        private static IDictionary<string, string> GetCustomers(DataService dataService)
        {
            var customerDictionary = new Dictionary<string, string>();
            var customers = dataService.FindAll(new Customer()).ToList();
            foreach (var customer in customers)
            {
                if (customer.FullyQualifiedName.Contains(":"))
                {
                    var subCompany = customer.FullyQualifiedName.Split(':');
                    customer.FullyQualifiedName = subCompany[1];
                }
                if (customer.BillAddr == null)
                {
                    customerDictionary.Add(customer.FullyQualifiedName, null);
                    continue;
                }
                customerDictionary.Add(customer.FullyQualifiedName, customer.BillAddr.CountrySubDivisionCode);
            }
            return customerDictionary;
        }

        private static void SetTaxCode(T entity)
        {
            foreach (var line in entity.Line)
            {
                var lineDetail = line.AnyIntuitObject as SalesItemLineDetail;
                if (lineDetail != null)
                {
                    if (lineDetail.TaxCodeRef == null) lineDetail.TaxCodeRef = new ReferenceType { Value = "TAX" };
                    lineDetail.TaxCodeRef.Value = "TAX";
                }
            }
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
            return taxRateList.ToDictionary(item => item.CountrySubDivisionCode, item => item.Tax);
        }

        private static string GetTaxRateId(ServiceContext context, decimal taxRate)
        {
            var taxValues = new QueryService<TaxRate>(context);
            var id = taxValues.Where(x => x.RateValue == taxRate && x.Active).Select(x => x.Id).FirstOrDefault();
            return id;
        }

        private static TaxService AddTaxService(ServiceContext context, decimal percent)
        {
            var queryService = new QueryService<TaxAgency>(context);
            var taxAgency = queryService.ExecuteIdsQuery("select * from TaxAgency").First();
            var name = $"{percent} percent tax";
            var taxRateDetailses = new[] { new TaxRateDetails { RateValue = percent, RateValueSpecified = true, TaxAgencyId = taxAgency.Id, TaxApplicableOn = TaxRateApplicableOnEnum.Sales, TaxRateName = name } };
            var taxService = new TaxService { TaxCode = name, TaxRateDetails = taxRateDetailses };
            context.IppConfiguration.Message.Request.SerializationFormat = SerializationFormat.Json;
            context.IppConfiguration.Message.Response.SerializationFormat = SerializationFormat.Json;
            var globalTaxService = new GlobalTaxService(context);
            globalTaxService.AddTaxCode(taxService);
            return taxService;
        }
    }
}
