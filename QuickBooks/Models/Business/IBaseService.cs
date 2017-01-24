using System.Collections.Generic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public interface IBaseService<T> where T: SalesTransaction
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="context">the entity of Intuit.Ipp.Core.ServiceContext.</param>
        /// <param name="notCalculatedEntities">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList<T> Recalculate(ServiceContext context, IList<T> notCalculatedEntities = null);
        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="context"> entity of Intuit.Ipp.Core.ServiceContext.</param>
        /// <param name="entity">entity Intuit.Ipp.WebhooksService.Entity.</param>
        void Update(ServiceContext context, Entity entity);
    }
}
