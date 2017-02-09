using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class CreditMemoService : BaseService<CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(IReportRepository reportRepository, ITaxRepository repository, IOAuthService oAuthService) : base(reportRepository, repository, oAuthService, "Credit Memo")
        {
        }
    }
}
