using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Common.Logging;
using Intuit.Ipp.WebhooksService;
using Newtonsoft.Json;

namespace QuickBooks.Models.Business
{
    public class NotificationService : INotificationService
    {
        public NotificationService(params IService[] services)
        {
            serviceDictionary = new Dictionary<string, IService>();
            foreach (var service in services)
            {
                serviceDictionary.Add(service.EntityName, service);
            }
        }
        private static readonly ILog Log = LogManager.GetLogger<NotificationService>();
        private static string _payloadLoaded;
        private readonly IDictionary<string, IService> serviceDictionary;
        private static readonly string Verifier = ConfigurationManager.AppSettings["WebHooksVerifier"];

        public bool VerifyPayload(string intuitHeader, string payload)
        {
            try
            {
                _payloadLoaded = payload;
                if (intuitHeader == null) return false;
                var keyBytes = Encoding.UTF8.GetBytes(Verifier);
                var dataBytes = Encoding.UTF8.GetBytes(_payloadLoaded);
                var hmac = new HMACSHA256(keyBytes);
                var hmacBytes = hmac.ComputeHash(dataBytes);
                var createPayloadSignature = Convert.ToBase64String(hmacBytes);
                return intuitHeader == createPayloadSignature;
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to process notifications", e);
                return false;
            }
        }

        public WebhooksEvent GetWebooksEvents(string payload)
        {
            var webhooksData = JsonConvert.DeserializeObject<WebhooksEvent>(payload);
            return webhooksData;
        }

        public void Process(string notifications)
        {
            try
            {
                var webhooksData = GetWebooksEvents(notifications);

                foreach (var notification in webhooksData.EventNotifications)
                {
                    foreach (var entity in notification.DataChangeEvent.Entities)
                    {
                        Update(entity);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to update data", e);
                throw;
            }
        }

        private void Update(Entity entity)
        {
            if (serviceDictionary.ContainsKey(entity.Name))
                serviceDictionary[entity.Name].Process(entity);
        }
    }
}