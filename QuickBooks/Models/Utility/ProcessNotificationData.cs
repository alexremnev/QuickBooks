using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json;

namespace QuickBooks.Models.Utility
{
    public class ProcessNotificationData
    {
        private static readonly ILog Log = LogManager.GetLogger<ProcessNotificationData>();
        private static string _payloadLoaded;
        public static bool Validate(string payload, object hmacHeaderSignature)
        {
            _payloadLoaded = payload;
            var verifier = ConfigurationManager.AppSettings["WebHooksVerifier"];
            if (hmacHeaderSignature == null) return false;
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(verifier);
                var dataBytes = Encoding.UTF8.GetBytes(_payloadLoaded);
                var hmac = new HMACSHA256(keyBytes);
                var hmacBytes = hmac.ComputeHash(dataBytes);
                var createPayloadSignature = Convert.ToBase64String(hmacBytes);
                if ((string)hmacHeaderSignature != createPayloadSignature) return false;
                var thread = new Thread(AddToQueue);
                thread.Start();
                thread.Join(60000);
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to process notifications", e);
                return false;
            }
        }
        private static void AddToQueue()
        {
            var webhooksData = JsonConvert.DeserializeObject<NotificationEntity.WebhooksData>(_payloadLoaded);
            var dataItems = new BlockingCollection<NotificationEntity.WebhooksData>(1);

            Task.Run(() =>
            {
                // Add items to blocking collection(queue)
                dataItems.Add(webhooksData);
                // Let consumer know we are done.
                dataItems.CompleteAdding();
            });

            Task.Run(() =>
            {
                while (!dataItems.IsCompleted)
                {
                    //Create WebhooksData reference
                    NotificationEntity.WebhooksData webhooksData1 = null;
                    try
                    {
                        //Take our Items from blocking collection(dequeue)
                        webhooksData1 = dataItems.Take();
                    }
                    catch (InvalidOperationException) { }

                    if (webhooksData1 != null)
                    {
                        //Start processing queue items
                        ProcessQueueItem(webhooksData1);
                    }
                }
            });
        }


        /// <summary>
        /// Process each item of queue
        /// </summary>  
        private static void ProcessQueueItem(NotificationEntity.WebhooksData queueItem1)
        {
            //Get realm from deserialized WebHooksData 
            foreach (var eventNotifications in queueItem1.EventNotifications)
            {
            }
        }
    }
}