using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface IInvoiceService : IBaseService<Invoice>
    {
        /// <summary>
        /// Save the list of Invoice in database.
        /// </summary>
        /// <param name="list">the list of invoice.</param>
        void Save(IList<Invoice> list = null);
    }
}
