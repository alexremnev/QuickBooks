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
        private readonly BaseEntity _baseEntity;
        protected BaseService(IReportService service, ITaxRepository taxRepository, BaseEntity baseEntity, string entityName)
        {
            _reportservice = service;
            _log = LogManager.GetLogger(GetType());
            _entityName = entityName;
            _taxRepository = taxRepository;
            _baseEntity = baseEntity;
        }

        private readonly IReportService _reportservice;
        private readonly ITaxRepository _taxRepository;
        private readonly string _entityName;

        public IList<T> Recalculate(ServiceContext context, IList<T> recalculateEntity = null)
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
                var entities = recalculateEntity ?? service.Select(x => x).ToList();
//              var entities = service.Where(x => x.DocNumber == 1007.ToString()).ToList();//todo it's test data
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

                foreach (var entity in entities)
                {
                    foreach (var line in entity.Line)
                    {
                        var lineDetail = line.AnyIntuitObject as SalesItemLineDetail;
                        if (lineDetail != null)
                        {
                            if (lineDetail.TaxCodeRef==null) lineDetail.TaxCodeRef =new ReferenceType() {Value = "TAX"};
                            lineDetail.TaxCodeRef.Value = "TAX";
                        }
                    }

                    switch (customerDictionary[entity.CustomerRef.name])
                    {
                        case "CA":
                            // entity.TxnTaxDetail.TxnTaxCodeRef.Value = 5.ToString();
                            entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType() { Value = 5.ToString() };
                            break;
                        case "NY":
                            entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType() { Value = 6.ToString() };
                            break;
                        default:
                            entity.TxnTaxDetail.TxnTaxCodeRef = new ReferenceType() { Value = 7.ToString() };
                            break;
                    }
                    
                   // dataService.Add(entity);
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
                    if (entity.Id != null) _baseEntity.Id = entity.Id;
                    if (entity.DocNumber != null) _baseEntity.DocNumber = entity.DocNumber;
                    _baseEntity.TxnDate = entity.TxnDate;
                    if (entity.BillAddr.Line1 != null) _baseEntity.NameAndId = entity.BillAddr.Line1;
                    var address = new StringBuilder();
                    if (entity.ShipAddr != null)
                    {
                        if (entity.ShipAddr.Line1 != null) address.Append(entity.ShipAddr.Line1);
                        if (entity.ShipAddr.City != null) address.Append(" " + entity.ShipAddr.City);
                        if (entity.ShipAddr.CountrySubDivisionCode != null)
                            address.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                        if (entity.ShipAddr.PostalCode != null) address.Append(", " + entity.ShipAddr.PostalCode);
                    }
                    _baseEntity.ShipAddr = address.ToString();

                    if (entity.Line == null)
                    {
                        _reportservice.Save(_baseEntity);
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
                    Save(recalculatedList);
                }
            }
        }

        private bool IsEqualLines(IList<Line> quickBookslines, IList<LineItem> actuaLines)
        {
            if (quickBookslines.Count != actuaLines.Count) return false;
            for (var i = 0; i < quickBookslines.Count; i++)
            {
                if (quickBookslines[i].Amount != actuaLines[i].Amount) return false;
            }
            return true;
        }
    }
}
