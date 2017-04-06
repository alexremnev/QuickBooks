using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
   public interface IRegistry<T>
    {
        IList<T> GetServices();
    }
}
