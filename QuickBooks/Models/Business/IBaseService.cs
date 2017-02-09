using System.Collections.Generic;
using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public interface IBaseService<T>
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="list">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList<T> Recalculate(IList<T> list = null);
        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="entity">entity Intuit.Ipp.WebhooksService.Entity.</param>
        void Update(Entity entity);
    }
}
