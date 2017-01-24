using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository() : base(NameEntity)
        {
        }
        private const string NameEntity = "Report";
    }
}