using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
    public interface IBaseService<T> : IService
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="list">entity which must be recalculated.</param>
        /// <returns>recalculated list of entities.</returns>
        IList<T> Recalculate(IList<T> list = null);
    }
}
