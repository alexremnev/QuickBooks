using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Common.Logging;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Security;
using QuickBooks.Models.EntityService;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILog _log = LogManager.GetLogger<ReportController>();
        private readonly ICreditMemoService _creditMemoService;
        private readonly IInvoiceService _invoiceService;
        public ReportController(ICreditMemoService creditMemoService, IInvoiceService invoiceService)
        {
            _creditMemoService = creditMemoService;
            _invoiceService = invoiceService;
        }

        public ActionResult Create()
        {
            try
            {
                var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
                var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
                var appToken = @"926acffab65abb4251b9d29b3d7daf2f4bf5";

                string realmId = System.Web.HttpContext.Current.Session["realm"].ToString();
                var intuitServicesType = IntuitServicesType.QBO;
                OAuthRequestValidator oauthValidator =
                    new OAuthRequestValidator(System.Web.HttpContext.Current.Session["accessToken"].ToString(),
                        System.Web.HttpContext.Current.Session["accessTokenSecret"].ToString(), consumerKey,
                        consumerSecret);
                ServiceContext context = new ServiceContext(appToken, realmId, intuitServicesType, oauthValidator);
                DataService dataService = new DataService(context);
                IList<CreditMemo> creditMemos = dataService.FindAll(new CreditMemo()).ToList();
                _creditMemoService.Save(creditMemos);
                IList<Invoice> invoices = dataService.FindAll(new Invoice()).ToList();
                _invoiceService.Save(invoices);
                return Json(invoices, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to pull entity", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
    }
}