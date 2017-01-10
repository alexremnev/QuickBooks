using System;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        private const string NameEntity = "Report";

        public ReportRepository() : base(NameEntity)
        {
        }

        }
}