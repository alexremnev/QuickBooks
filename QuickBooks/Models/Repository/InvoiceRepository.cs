using Common.Logging;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        private static readonly ILog Log = LogManager.GetLogger<InvoiceRepository>();
        private const string NameEntity = "Invoice";

        public InvoiceRepository() : base(Log, NameEntity)
        {
        }
    }
}