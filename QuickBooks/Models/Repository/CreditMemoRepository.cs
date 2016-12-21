using Common.Logging;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class CreditMemoRepository : BaseRepository<CreditMemo>, ICreditMemoRepository
    {
        private static readonly ILog Log = LogManager.GetLogger<CreditMemoRepository>();
        private const string NameEntity = "Credit Memo";

        public CreditMemoRepository() : base(Log, NameEntity)
        {
        }
    }
}