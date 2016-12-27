using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
    public interface ICreditMemoService : IBaseService
    {
        /// <summary>
        /// Save the list of Credit Memo in database.
        /// </summary>
        /// <param name="entities">the list of Credit Memo.</param>
        void Save(IList<Intuit.Ipp.Data.CreditMemo> entities);
    }
}
