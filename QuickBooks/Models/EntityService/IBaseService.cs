using System.Collections.Generic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.EntityService
{
    public interface IBaseService<T> where T: SalesTransaction
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="context">the entity of Intuit.Ipp.Core.ServiceContext.</param>
        void Recalculate(ServiceContext context);
    }
}
