using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
    public interface IBaseService<T> : IPersisting
    {
        /// <summary>
        /// Calculate sales tax.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <param name="list">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList<T> Calculate(string realmId, IList<T> list = null);
    }
}
