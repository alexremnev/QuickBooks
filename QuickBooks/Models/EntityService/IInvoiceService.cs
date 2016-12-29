﻿using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
    public interface IInvoiceService : IBaseService
    {
        /// <summary>
        /// Save the list of Invoice in database.
        /// </summary>
        /// <param name="entities"> the list of invoice.</param>
        ///  <param name="accountingMethod">Accounting method. Can be Cash or Accrual.</param>
        void Save(IList<Intuit.Ipp.Data.Invoice> entities, string accountingMethod = "Accrual");
    }
}
