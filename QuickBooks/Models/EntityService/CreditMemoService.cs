using System.Collections.Generic;
using System.Text;
using Intuit.Ipp.Data;
using QuickBooks.Models.DAL;
using QuickBooks.Models.ReportService;
using CreditMemo = QuickBooks.Models.DAL.CreditMemo;


namespace QuickBooks.Models.EntityService
{
    public class CreditMemoService : ICreditMemoService
    {
        private readonly IReportService _service;
        public CreditMemoService(IReportService service)
        {
            _service = service;
        }

        public void Save(IList<Intuit.Ipp.Data.CreditMemo> entities)
        {
            foreach (var entity in entities)
            {
                var lineItems = new List<LineItem>();
                var creditMemo = new CreditMemo();
                if (entity.DocNumber != null) creditMemo.DocNumber = entity.DocNumber;
                creditMemo.TxnDate = entity.TxnDate;
                if (entity.BillAddr.Line1 != null) creditMemo.NameAndId = entity.BillAddr.Line1;
                var adress = new StringBuilder();
                if (entity.ShipAddr != null)
                {
                    if (entity.ShipAddr.Line1 != null) adress.Append(entity.ShipAddr.Line1);
                    if (entity.ShipAddr.City != null) adress.Append(" " + entity.ShipAddr.City);
                    if (entity.ShipAddr.CountrySubDivisionCode != null) adress.Append(" " + entity.ShipAddr.CountrySubDivisionCode);
                    if (entity.ShipAddr.PostalCode != null) adress.Append(", " + entity.ShipAddr.PostalCode);
                }
                creditMemo.ShipAddr = adress.ToString();
                
                if (entity.Line == null)
                {
                   _service.Save(creditMemo);
                    continue;
                }

                foreach (var line in entity.Line)
                {
                    var lineItem = new LineItem();
                    if (line == null) continue;
                    if (line.Amount != 0) lineItem.Amount = line.Amount;
                    if (!(line.AnyIntuitObject is SalesItemLineDetail)) continue;
                    lineItem.Quantity = (int)((SalesItemLineDetail)line.AnyIntuitObject).Qty;
                    if (((SalesItemLineDetail) line.AnyIntuitObject).ItemRef == null) continue;
                    lineItem.Name = ((SalesItemLineDetail)line.AnyIntuitObject).ItemRef.name;

                    lineItems.Add(lineItem);
                }
                creditMemo.LineItems = lineItems;
                _service.Save(creditMemo);
            }
        }
    }
}
