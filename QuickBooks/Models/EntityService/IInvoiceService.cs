using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.EntityService
{
    public interface IInvoiceService : IBaseService
    {
        /// <summary>
        /// Save the list of Invoice in database.
        /// </summary>
        /// <param name="entities"> the list of invoice.</param>
        ///  <param name="accountingMethod">Enumeration of Tax Report Basis for France.</param>
        void Save(IList<Intuit.Ipp.Data.Invoice> entities, ReportBasisEnum accountingMethod = ReportBasisEnum.Accrual);
    }
}
