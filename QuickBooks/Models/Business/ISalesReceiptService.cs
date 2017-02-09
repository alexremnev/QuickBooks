using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface ISalesReceiptService : IBaseService<SalesReceipt>
    {
        /// <summary>
        /// Save the list of Sales Receipt in database.
        /// </summary>
        /// <param name="entities">the list of sales receipt.</param>
        void Save(IList<SalesReceipt> list = null);
    }
}
