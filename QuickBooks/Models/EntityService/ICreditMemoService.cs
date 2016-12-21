using System.Collections.Generic;

namespace QuickBooks.Models.EntityService
{
   public interface ICreditMemoService
   {
       void Save(IList<Intuit.Ipp.Data.CreditMemo> entities);
   }
}
