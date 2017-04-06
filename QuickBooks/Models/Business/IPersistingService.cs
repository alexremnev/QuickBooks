namespace QuickBooks.Models.Business
{
    public interface IPersistingService 
    {
        /// <summary>
        /// Persist all entities from QuickBooks in database.
        /// </summary>
        /// <param name="realmId">entity realmId.</param>
        void Save(string realmId);
    }
}