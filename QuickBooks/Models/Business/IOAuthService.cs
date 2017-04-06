using System.Collections.Generic;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Intuit.Ipp.GlobalTaxService;
using Intuit.Ipp.QueryFilter;
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
        /// Get ServiceContext
        /// </summary>
        /// <returns>return ServiceContext or null.</returns>
        ServiceContext GetContext(string realmId);

        /// <summary>
        /// Get QueryService.
        /// </summary>
        /// <returns>return new QueryService or null.</returns>
        QueryService<T> GetQueryService<T>(string realmId);

        /// <summary>
        /// Get DataService.
        /// </summary>
        /// <returns>return new DataService or null.</returns>
        DataService GetDataService(string realmId);

        /// <summary>
        /// Get GlobalTaxService.
        /// </summary>
        /// <returns>Return GlobalTaxService or null.</returns>
        GlobalTaxService GetGlobalTaxService(string realmId);
    }
}