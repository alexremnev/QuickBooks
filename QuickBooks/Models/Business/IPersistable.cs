using System.Collections;

namespace QuickBooks.Models.Business
{
    public interface IPersistable : IService
    {
        /// <summary>
        /// Save the list of entities in database.
        /// </summary>
        /// <param name="list">the list of entities.</param>
        void Save(IList list = null);
    }
}
