using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IOAuthRepository : IBaseRepository<OAuth>
    {
        /// <summary>
        /// Return the entity of Aouth.
        /// </summary>
        /// <returns>returns the entity of OAuth.</returns>
        OAuth Get();
    }
}