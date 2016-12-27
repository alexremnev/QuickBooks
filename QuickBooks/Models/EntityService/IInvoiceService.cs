using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
    public interface IInvoiceService : IBaseService
    {
        /// <summary>
        /// Save the list of Invoice in database.
        /// </summary>
        /// <param name="entities"> the list of invoice.</param>
        ///  <param name="accrualMethod">Accounting method</param>
        void Save(IList<Intuit.Ipp.Data.Invoice> entities, string accrualMethod = "Cash");
    }
}
