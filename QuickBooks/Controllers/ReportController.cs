using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Common.Logging;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Security;
using QuickBooks.Models.EntityService;
using CreditMemo = Intuit.Ipp.Data.CreditMemo;
using Invoice = Intuit.Ipp.Data.Invoice;
using SalesReceipt = Intuit.Ipp.Data.SalesReceipt;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILog _log = LogManager.GetLogger<ReportController>();
        private readonly ICreditMemoService _creditMemoService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IOAuthService _oauthService;
        private readonly IEstimateService _estimateSrService;
        private readonly string _appToken = ConfigurationManager.AppSettings["appToken"];
        private readonly string _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private readonly string _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private const IntuitServicesType IntuitServicesType = Intuit.Ipp.Core.IntuitServicesType.QBO;

        public ReportController(ICreditMemoService creditMemoService, IInvoiceService invoiceService,
            ISalesReceiptService salesReceiptService, IOAuthService oauthService, IEstimateService estimateService)
        {
            _creditMemoService = creditMemoService;
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _oauthService = oauthService;
            _estimateSrService = estimateService;
        }

        public ActionResult Create()
        {
            try
            {
                var permission = _oauthService.Get();
                var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                    permission.AccessTokenSecret, _consumerKey, _consumerSecret);
                var context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                    oauthValidator);
                var dataService = new DataService(context);
                var creditMemos = dataService.FindAll(new CreditMemo()).ToList();
                //  _creditMemoService.Save(creditMemos);
                var invoices = dataService.FindAll(new Invoice()).ToList();
                //  _invoiceService.Save(invoices);
                var sales = dataService.FindAll(new SalesReceipt()).ToList();
                // _salesReceiptService.Save(sales);
                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to pull entity", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Recalculate()
        {
            try
            {
                var permission = _oauthService.Get();
                var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                    permission.AccessTokenSecret, _consumerKey, _consumerSecret);
                var context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                    oauthValidator);
                context.IppConfiguration.Message.Request.SerializationFormat =
                    Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
                context.IppConfiguration.Message.Response.SerializationFormat =
                    Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
                var dataService = new DataService(context);

                _invoiceService.Recalculate(context, dataService);
                //                _creditMemoService.Recalculate(context, dataService);
                //                _salesReceiptService.Recalculate(context, dataService);
                //                _estimateSrService.Recalculate(context, dataService);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to recalculate sales tax", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
    }
}