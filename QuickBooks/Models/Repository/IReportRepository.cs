using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IReportRepository
    {
        void Create(Report entity);
    }
}
