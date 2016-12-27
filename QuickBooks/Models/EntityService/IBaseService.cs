using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;

namespace QuickBooks.Models.EntityService
{
    public interface IBaseService
    {
        /// <summary>
        /// Recalculate sales tax.
        /// </summary>
        /// <param name="context">the entity of Intuit.Ipp.Core.ServiceContext.</param>
        /// <param name="dataService">the entity of Intuit.Ipp.DataService.DataService.</param>
        void Recalculate(ServiceContext context, DataService dataService);
    }
}
