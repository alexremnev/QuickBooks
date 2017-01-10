using System.Collections;
using System.Collections.Generic;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface ITaxRepository
    {
        /// <summary>
        /// Get tax rate depends on state.
        /// </summary>
        /// <param name="state">entity state.</param>
        /// <returns>returns the entity of taxRate</returns>
        TaxRate GetByCountrySubDivisionCode(string state);
        /// <summary>
        /// Get list of taxRate.
        /// </summary>
        /// <returns>returns list of taxRate</returns>
        IList<TaxRate> List();
    }
}