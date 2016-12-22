using System.Collections.Generic;
using System.Text;
using Intuit.Ipp.Data;
using QuickBooks.Models.DAL;
using QuickBooks.Models.ReportService;
using SalesReceipt = QuickBooks.Models.DAL.SalesReceipt;

namespace QuickBooks.Models.EntityService
{
    public class SalesReceiptService: ISalesReceiptService
    {
        private readonly IReportService _service;
        public SalesReceiptService(IReportService service)
        {
            _service = service;
        }

        public void Save(IList<Intuit.Ipp.Data.SalesReceipt> entities)
        {
            foreach (var entity in entities)
            {
                var lineItems = new List<LineItem>();
                var salesReceipt = new SalesReceipt();
                if (entity.DocNumber != null) salesReceipt.DocNumber = entity.DocNumber;
                salesReceipt.TxnDate = entity.TxnDate;
                if (entity.BillAddr.Line1 != null) salesReceipt.NameAndId = entity.BillAddr.Line1;
                var adress = new StringBuilder();
                if (entity.ShipAddr != null)
                {
                    if (entity.ShipAddr.Line1 != null) adress.Append(entity.ShipAddr.Line1);
                    if (entity.ShipAddr.City != null) adress.Append(" " + entity.ShipAddr.City);
                    if (entity.ShipAddr.CountrySubDivisionCode != null) adress.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                    if (entity.ShipAddr.PostalCode != null) adress.Append(", " + entity.ShipAddr.PostalCode);
                }
                salesReceipt.ShipAddr = adress.ToString();

                if (entity.Line == null)
                {
                    _service.Save(salesReceipt);
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
                salesReceipt.LineItems = lineItems;
                _service.Save(salesReceipt);
            }
        }
    }
}