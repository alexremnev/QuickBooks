using System.Collections.Generic;
using QuickBooks.Models.ReportService;
using CreditMemo = QuickBooks.Models.DAL.CreditMemo;


namespace QuickBooks.Models.EntityService
{
    public class CreditMemoService : BaseService<Intuit.Ipp.Data.CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(IReportService service) : base(service, "Credit Memo")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.CreditMemo> entities)
        {
            var baseEntity = new CreditMemo();
            Save(entities, baseEntity);
        }
    }
}
