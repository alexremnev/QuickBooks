using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IOAuthRepository : IBaseRepository<OAuth>
    {
        /// <summary>
        /// Return the entity of Aouth.
        /// </summary>
        /// <param name="realmId">entity realmId</param>
        /// <returns>returns the entity of OAuth.</returns>
        OAuth Get(string realmId);
    }
}