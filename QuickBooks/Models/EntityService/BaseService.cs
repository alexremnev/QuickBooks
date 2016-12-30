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

namespace QuickBooks.Models.EntityService
{
    public abstract class BaseService<T> : IBaseService where T : SalesTransaction
    {
        private readonly ILog _log;
        protected BaseService(IReportService service, string entityName)
        {
            _service = service;
            _log = LogManager.GetLogger(GetType());
            _entityName = entityName;
        }

        private readonly IReportService _service;
        private readonly string _entityName;

        public void Recalculate(ServiceContext context, DataService dataService)
        {
            try
            {
                var service = new QueryService<T>(context);
                var entities = service.Select(x => x).ToList();
                var stateTaxCodeQueryService = new QueryService<TaxCode>(context);
                var stateTaxCodes =
                    stateTaxCodeQueryService.ExecuteIdsQuery("Select * From TaxCode").ToList();

                foreach (var entity in entities)
                {
                    TaxCode stateTaxCode;
                    string state = null;
                    if (entity.ShipAddr != null) state = entity.ShipAddr.CountrySubDivisionCode;
                    else if (entity.BillAddr != null)
                    {
                        state = entity.BillAddr.CountrySubDivisionCode;
                        if (state == null)
                        {
                            if (entity.BillAddr.Line4.Contains("CA")) state = "CA";
                            else if (entity.BillAddr.Line4.Contains("NY")) state = "NY";
                        }
                    }

                    switch (state)
                    {
                        case "CA":
                            {
                                stateTaxCode = stateTaxCodes[3];
                                break;
                            }
                        case "NY":
                            {
                                stateTaxCode = stateTaxCodes[7];
                                break;
                            }
                        default:
                            {
                                stateTaxCode = stateTaxCodes[6];
                                break;
                            }
                    }

                    var txnTaxDetail = new TxnTaxDetail
                    {
                        TxnTaxCodeRef = new ReferenceType
                        {
                            name = stateTaxCode.Name,
                            Value = stateTaxCode.Id
                        }
                    };
                    var taxLine = new Line { DetailType = LineDetailTypeEnum.TaxLineDetail };
                    var taxLineDetail = new TaxLineDetail
                    {
                        TaxRateRef = stateTaxCode.SalesTaxRateList.TaxRateDetail[0].TaxRateRef
                    };
                    taxLine.AnyIntuitObject = taxLineDetail;
                    txnTaxDetail.TaxLine = new[] { taxLine };
                    entity.TxnTaxDetail = txnTaxDetail;
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
                    if (entity.DocNumber != null) baseEntity.DocNumber = entity.DocNumber;
                    baseEntity.TxnDate = entity.TxnDate;
                    if (entity.BillAddr.Line1 != null) baseEntity.NameAndId = entity.BillAddr.Line1;
                    var adress = new StringBuilder();
                    if (entity.ShipAddr != null)
                    {
                        if (entity.ShipAddr.Line1 != null) adress.Append(entity.ShipAddr.Line1);
                        if (entity.ShipAddr.City != null) adress.Append(" " + entity.ShipAddr.City);
                        if (entity.ShipAddr.CountrySubDivisionCode != null)
                            adress.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                        if (entity.ShipAddr.PostalCode != null) adress.Append(", " + entity.ShipAddr.PostalCode);
                    }
                    baseEntity.ShipAddr = adress.ToString();

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