using System.Collections.Generic;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;
using SalesReceipt = QuickBooks.Models.DAL.SalesReceipt;

namespace QuickBooks.Models.EntityService
{
    public class SalesReceiptService : BaseService<Intuit.Ipp.Data.SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportService service, ITaxRepository repository) : base(service, repository, "Sales Receipt")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.SalesReceipt> entities)
        {
            var baseEntity = new SalesReceipt();
            Save(entities, baseEntity);
        }
    }
}