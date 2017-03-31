using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository() : base(NameEntity)
        {
        }
        private const string NameEntity = "Report";
    }
}