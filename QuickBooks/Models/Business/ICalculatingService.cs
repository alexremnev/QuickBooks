using System.Collections;

namespace QuickBooks.Models.Business
{
    public interface ICalculatingService
    {
        /// <summary>
        /// Calculate sales tax in all documents.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <returns>recalculated list of entities.</returns>
        IList Calculate(string realmId);
    }
}