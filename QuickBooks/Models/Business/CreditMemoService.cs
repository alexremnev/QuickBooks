using QuickBooks.Models.Repository;
using CreditMemo = QuickBooks.Models.DAL.CreditMemo;

namespace QuickBooks.Models.Business
{
    public class CreditMemoService : BaseService<Intuit.Ipp.Data.CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(IReportService service, ITaxRepository repository) : base(service, repository, new CreditMemo(), "Credit Memo")
        {
        }
    }
}
