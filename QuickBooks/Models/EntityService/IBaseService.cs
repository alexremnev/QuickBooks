using System.Collections.Generic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.EntityService
{
    public interface IBaseService<T> where T: SalesTransaction
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="context">the entity of Intuit.Ipp.Core.ServiceContext.</param>
        /// <param name="recalculateEntity">entity which must be recalculated</param>
        /// <returns>recalculated list of entities</returns>
        IList<T> Recalculate(ServiceContext context, IList<T> recalculateEntity = null);

        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="context">the entity of Intuit.Ipp.Core.ServiceContext.</param>
        /// <param name="entity">entity NotificationEntity.Entities</param>
        void Update(ServiceContext context, NotificationEntity.Entities entity);
    }
}
