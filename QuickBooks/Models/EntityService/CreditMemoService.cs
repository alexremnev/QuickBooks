using System.Collections.Generic;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;
using CreditMemo = QuickBooks.Models.DAL.CreditMemo;


namespace QuickBooks.Models.EntityService
{
    public class CreditMemoService : BaseService<Intuit.Ipp.Data.CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(IReportService service, ITaxRepository repository) : base(service, repository, "Credit Memo")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.CreditMemo> entities)
        {
            var baseEntity = new CreditMemo();
            Save(entities, baseEntity);
        }
    }
}
