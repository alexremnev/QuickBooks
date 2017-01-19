using Intuit.Ipp.Core;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.EntityService
{
    public interface IOAuthService
    {
        /// <summary>
        /// Save the realmId, accessToken and accessToken secret in database.
        /// </summary>
        /// <param name="entity">entity of OAuth</param>
        void Save(OAuth entity);
        /// <summary>
        /// Get the entity of OAuth from database.
        /// </summary>
        /// <param name="realmId">entity realmId</param>
        /// <returns>found OAuth entity or null.</returns>
        OAuth Get(string realmId);
        /// <summary>
        /// Get ServiceContext
        /// </summary>
        /// <returns>return ServiceContext or null.</returns>
        ServiceContext GetServiceContext();
        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">entity id.</param>
        void Delete(string id);

    }
}
