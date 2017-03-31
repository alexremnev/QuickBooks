using System.Collections;

namespace QuickBooks.Models.Business
{
    public interface IPersisting : IService
    {
        /// <summary>
        /// Create the list of entities in database.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        /// <param name="list">the list of entities.</param>
        void Save(string realmId, IList list = null);
    }
}
