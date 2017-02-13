using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Intuit.Ipp.GlobalTaxService;
using Intuit.Ipp.QueryFilter;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Business
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
        /// <summary>
        /// Get ServiceContext
        /// </summary>
        /// <returns>return ServiceContext or null.</returns>
        ServiceContext GetContext();
        /// <summary>
        /// Delete entity.
        /// </summary>
        void Delete();
        /// <summary>
        /// Get QueryService.
        /// </summary>
        /// <returns>return new QueryService or null.</returns>
        QueryService<T> GetQueryService<T>();
        /// <summary>
        /// Get DataService.
        /// </summary>
        /// <returns>return new DataService or null.</returns>
        DataService GetDataService();
        /// <summary>
        /// Get GlobalTaxService.
        /// </summary>
        /// <returns>Return GlobalTaxService or null.</returns>
        GlobalTaxService GetGlobalTaxService();
    }
}
