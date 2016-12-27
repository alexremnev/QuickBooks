using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
    public interface ISalesReceiptService : IBaseService
    {
        /// <summary>
        /// Save the list of Sales Receipt in database.
        /// </summary>
        /// <param name="entities">the list of sales receipt.</param>
        void Save(IList<Intuit.Ipp.Data.SalesReceipt> entities);
    }
}
