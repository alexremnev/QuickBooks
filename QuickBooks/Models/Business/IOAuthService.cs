using System.Collections.Generic;
using QuickBooks.Models.Data;

namespace QuickBooks.Models.Business
{
    public interface IOAuthService
    {
        /// <summary>
        /// Create the realmId, accessToken and accessToken secret in database.
        /// </summary>
        /// <param name="entity">entity of OAuth</param>
        void Save(OAuth entity);

        /// <summary>
        /// Delete entity from database.
        /// </summary>
        /// <param name="realmId">entity realmId</param>
        void Delete(string realmId);

        /// <summary>
        /// Rerutns list of Oauth.
        /// </summary>
        /// <returns>list of Oauth or null in case if entities not found.</returns>
        IList<OAuth> List();
       
        /// <summary>
        /// Returns entity Oauth.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <returns>entity Oauth.</returns>
        OAuth Get(string realmId);
    }
}