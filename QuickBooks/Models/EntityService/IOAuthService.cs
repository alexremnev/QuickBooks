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
        /// <returns>found OAuth entity or null.</returns>
       OAuth Get();
   }
}
