using QuickBooks.Models.EntityService;

namespace QuickBooks.Models.Utility
{
   public interface IProcessNotificationData
    {
        /// <summary>
        /// Verifies notifications authenticity.
        /// </summary>
        /// <param name="payload">incoming notifications.</param>
        /// <param name="hmacHeaderSignature">header signature.</param>
        /// <returns>return true if notifications genuine.</returns>
        bool Validate(string payload, object hmacHeaderSignature);
        /// <summary>
        /// Update data in database.
        /// </summary>
        /// <param name="notifications">incoming notifications.</param>
        /// <param name="oAuthService">entity OAuthService.</param>
        void Update(string notifications, IOAuthService oAuthService);
    }
}
