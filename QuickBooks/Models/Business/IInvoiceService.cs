using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface IInvoiceService : IBaseService<Invoice>
    {
        /// <summary>
        /// Create the list of Invoice in database.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <param name="list">the list of invoice.</param>
        void Save(string realmId, IList<Invoice> list = null);
    }
}
