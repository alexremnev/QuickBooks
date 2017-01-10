using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using QuickBooks.Models.DAL;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public abstract class BaseService<T> : IBaseService<T> where T : SalesTransaction
    {
        private readonly ILog _log;
        protected BaseService(IReportService service, ITaxRepository taxRepository, string entityName)
        {
            _service = service;
            _log = LogManager.GetLogger(GetType());
            _entityName = entityName;
            _taxRepository = taxRepository;
        }

        private readonly IReportService _service;
        private readonly ITaxRepository _taxRepository;
        private readonly string _entityName;

        public void Recalculate(ServiceContext context)
        {
            try
            {
                var taxRateDictionary = new Dictionary<string, decimal>();
                var taxRateList = _taxRepository.List();
                foreach (var item in taxRateList)
                {
                    taxRateDictionary.Add(item.CountrySubDivisionCode, item.Tax);
                }
                //------------------------------------------------------
                //ADD calculating tax sales using tax rate from database.
                //------------------------------------------------------
                var dataService = new DataService(context);
                var service = new QueryService<T>(context);
                var entities = service.Select(x => x).ToList();
                //                var entities = service.Where(x => x.DocNumber == 1004.ToString()).ToList();//todo it's test data
                var customerDictionary = new Dictionary<string, string>();
                var customers = dataService.FindAll(new Customer()).ToList();
                foreach (var customer in customers)
                {
                    if (customer.FullyQualifiedName.Contains(":"))
                    {
                        var subCompany = customer.FullyQualifiedName.Split(':');
                        customer.FullyQualifiedName = subCompany[1];
                    }
                    if (customer.BillAddr == null) { customerDictionary.Add(customer.FullyQualifiedName, null); continue; }
                    customerDictionary.Add(customer.FullyQualifiedName, customer.BillAddr.CountrySubDivisionCode);
                }

                foreach (var entity in entities)
                {
                    foreach (var line in entity.Line)
                    {
                        var lineDetail = line.AnyIntuitObject as SalesItemLineDetail;
                        if (lineDetail != null)
                        {
                            lineDetail.TaxCodeRef.Value = "TAX";
                        }
                    }

                    switch (customerDictionary[entity.CustomerRef.name])
                    {
                        case "CA":
                            entity.TxnTaxDetail.TxnTaxCodeRef.Value = 5.ToString();
                            break;
                        case "NY":
                            entity.TxnTaxDetail.TxnTaxCodeRef.Value = 6.ToString();
                            break;
                        default:
                            entity.TxnTaxDetail.TxnTaxCodeRef.Value = 7.ToString();
                            break;
                    }

                    dataService.Add(entity);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when you tried to recalculate sales tax in {_entityName}", e);
                throw;
            }
        }

        public void Save(IList<T> entities, BaseEntity baseEntity)
        {
            try
            {
                foreach (var entity in entities)
                {
                    var lineItems = new List<LineItem>();
                    if (entity.Id != null) baseEntity.Id = entity.Id;
                    if (entity.DocNumber != null) baseEntity.DocNumber = entity.DocNumber;
                    baseEntity.TxnDate = entity.TxnDate;
                    if (entity.BillAddr.Line1 != null) baseEntity.NameAndId = entity.BillAddr.Line1;
                    var address = new StringBuilder();
                    if (entity.ShipAddr != null)
                    {
                        if (entity.ShipAddr.Line1 != null) address.Append(entity.ShipAddr.Line1);
                        if (entity.ShipAddr.City != null) address.Append(" " + entity.ShipAddr.City);
                        if (entity.ShipAddr.CountrySubDivisionCode != null)
                            address.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                        if (entity.ShipAddr.PostalCode != null) address.Append(", " + entity.ShipAddr.PostalCode);
                    }
                    baseEntity.ShipAddr = address.ToString();

                    if (entity.Line == null)
                    {
                        _service.Save(baseEntity);
                        continue;
                    }

                    foreach (var line in entity.Line)
                    {
                        var lineItem = new LineItem();
                        if (line == null) continue;
                        if (line.Amount != 0) lineItem.Amount = line.Amount;
                        if (!(line.AnyIntuitObject is SalesItemLineDetail)) continue;
                        lineItem.Quantity = (int)((SalesItemLineDetail)line.AnyIntuitObject).Qty;
                        if (((SalesItemLineDetail)line.AnyIntuitObject).ItemRef == null) continue;
                        lineItem.Name = ((SalesItemLineDetail)line.AnyIntuitObject).ItemRef.name;
                        lineItems.Add(lineItem);
                    }
                    baseEntity.LineItems = lineItems;
                    _service.Save(baseEntity);
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when you tried to save {_entityName}", e);
                throw;
            }
        }
    }
}