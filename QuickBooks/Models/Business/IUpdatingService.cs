using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public interface IUpdatingService
    {
        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="realmId">entityt realmId.</param>
        /// <param name="entity">entity Intuit.Ipp.WebhooksService.Entity.</param>
        void Update(string realmId, Entity entity);
        /// <summary>
        /// Name of updated class entity 
        /// </summary>
        string EntityName { get; }
    }
}