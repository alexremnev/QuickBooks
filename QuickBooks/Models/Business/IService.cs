using System.Collections;
using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public interface IService
    {
        /// <summary>
        /// Calculate sales tax.
        /// </summary>
        /// <param name="list">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList Calculate(IList list = null);
        /// <summary>
        /// Process data.
        /// </summary>
        /// <param name="entity">entity Intuit.Ipp.WebhooksService.Entity.</param>
        void Process(Entity entity);
        string EntityName { get; }
    }
}