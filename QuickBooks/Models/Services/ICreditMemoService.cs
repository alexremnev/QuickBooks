using System.Collections.Generic;

namespace QuickBooks.Models.Services
{
   public interface ICreditMemoService
   {
       void Save(IList<Intuit.Ipp.Data.CreditMemo> entities);
   }
}
