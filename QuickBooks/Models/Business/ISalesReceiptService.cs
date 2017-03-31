using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface ISalesReceiptService : IBaseService<SalesReceipt>
    {
        /// <summary>
        /// Create the list of Sales Receipt in database.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <param name="list">the list of sales receipt.</param>
        void Save(string realmId, IList<SalesReceipt> list = null);
    }
}
