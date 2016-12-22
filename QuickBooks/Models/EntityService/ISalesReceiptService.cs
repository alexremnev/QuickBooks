using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
   public interface ISalesReceiptService
    {
        void Save(IList<Intuit.Ipp.Data.SalesReceipt> entities);
    }
}
