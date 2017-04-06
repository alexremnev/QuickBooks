using System;
using System.Collections.Generic;
using Common.Logging;
using Intuit.Ipp.WebhooksService;

namespace QuickBooks.Models.Business
{
    public class NotificationService : WebhooksService, INotificationService
    {
        public NotificationService(IUpdatingServicesRegistry updatingServicesRegistry)
        {
            var updatingServices = updatingServicesRegistry.GetServices();
            serviceDictionary = new Dictionary<string, IUpdatingService>();
            foreach (var service in updatingServices)
            {
                serviceDictionary.Add(service.EntityName, service);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger<NotificationService>();
        private readonly IDictionary<string, IUpdatingService> serviceDictionary;

        public void Process(string notifications)
        {
            try
            {
                var webhooksData = GetWebooksEvents(notifications);

                foreach (var notification in webhooksData.EventNotifications)
                {
                    var realmId = notification.RealmId;
                    foreach (var entity in notification.DataChangeEvent.Entities)
                    {
                        Update(realmId, entity);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to update data", e);
                throw;
            }
        }

        private void Update(string realmId, Entity entity)
        {
            if (serviceDictionary.ContainsKey(entity.Name))
                serviceDictionary[entity.Name].Update(realmId, entity);
        }
    }
}