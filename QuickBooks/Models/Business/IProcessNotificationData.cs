using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public interface IProcessNotificationData : IWebhooksService
    {
        /// <summary>
        /// Update data in database.
        /// </summary>
        /// <param name="notifications">incoming notifications.</param>
        /// <param name="oAuthService">entity OAuthService.</param>
        void Update(string notifications, IOAuthService oAuthService);
    }
}
