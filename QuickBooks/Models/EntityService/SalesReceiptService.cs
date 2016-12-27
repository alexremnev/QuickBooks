using System.Collections.Generic;
using QuickBooks.Models.ReportService;
using SalesReceipt = QuickBooks.Models.DAL.SalesReceipt;

namespace QuickBooks.Models.EntityService
{
    public class SalesReceiptService : BaseService<Intuit.Ipp.Data.SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportService service) : base(service, "Sales Receipt")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.SalesReceipt> entities)
        {
            var baseEntity = new SalesReceipt();
            Save(entities, baseEntity);
        }
    }
}