using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IOAuthRepository : IBaseRepository<OAuth>
    {
        /// <summary>
        /// Get Oauth.
        /// </summary>
        /// <returns>found entity or null in case there's no entity.</returns>
        OAuth Get();
        /// <summary>
        /// Delete an entity.
        /// </summary>
        void Delete();
    }
}