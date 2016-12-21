using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
    public interface IInvoiceService
    {
        void Save(IList<Intuit.Ipp.Data.Invoice> entities);
    }
}
