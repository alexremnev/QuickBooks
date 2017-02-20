using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
    public interface IBaseService<T> : IPersistable
    {
        /// <summary>
        /// Calculate sales tax.
        /// </summary>
        /// <param name="list">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList<T> Calculate(IList<T> list = null);
    }
}
