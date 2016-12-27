using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IReportRepository
    {
        /// <summary>
        /// Save the Report entity in database.
        /// </summary>
        /// <param name="entity">the Report entity which must be saved in database.</param>
        void Create(Report entity);
    }
}
