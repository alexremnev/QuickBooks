using System;
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
        public NotificationService(IInvoiceService invoiceService, ISalesReceiptService salesReceiptService,
           IEstimateService estimateService, ICreditMemoService creditMemoService)
        {
           
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _estimateService = estimateService;
            _creditMemoService = creditMemoService;
        }
        private static readonly ILog Log = LogManager.GetLogger<NotificationService>();
        private static string _payloadLoaded;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IEstimateService _estimateService;
        private readonly ICreditMemoService _creditMemoService;
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
                        switch (entity.Name)
                        {
                            case "Invoice":
                                _invoiceService.Update(entity);
                                break;
                            case "CreditMemo":
                                _creditMemoService.Update(entity);
                                break;
                            case "Estimate":
                                _estimateService.Update(entity);
                                break;
                            case "SalesReceipt":
                                _salesReceiptService.Update(entity);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to update data", e);
                throw;
            }
        }
    }
}