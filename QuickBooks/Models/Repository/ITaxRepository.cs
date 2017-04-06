using System.Collections.Generic;
using QuickBooks.Models.Data;

namespace QuickBooks.Models.Repository
{
    public interface ITaxRepository
    {
        /// <summary>
        /// Get list of taxRate.
        /// </summary>
        /// <returns>returns list of taxRate or null in case there's no entity found.</returns>
        IList<TaxRate> List();
    }
}