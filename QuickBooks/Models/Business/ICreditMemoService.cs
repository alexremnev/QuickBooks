using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface ICreditMemoService : IBaseService<CreditMemo>
    {
        /// <summary>
        /// Save the list of Credit Memo in database.
        /// </summary>
        /// <param name="entities">the list of Credit Memo.</param>
        void Save(IList<CreditMemo> entities);
    }
}
