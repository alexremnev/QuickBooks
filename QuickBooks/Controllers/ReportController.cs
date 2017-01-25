using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Common.Logging;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using QuickBooks.Models.Business;
using CreditMemo = Intuit.Ipp.Data.CreditMemo;
using Invoice = Intuit.Ipp.Data.Invoice;
using SalesReceipt = Intuit.Ipp.Data.SalesReceipt;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        public ReportController(ICreditMemoService creditMemoService, IInvoiceService invoiceService,
           ISalesReceiptService salesReceiptService, IOAuthService oauthService, IEstimateService estimateService)
        {
            _creditMemoService = creditMemoService;
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _oauthService = oauthService;
            _estimateSrService = estimateService;
        }
        private readonly ILog _log = LogManager.GetLogger<ReportController>();
        private readonly ICreditMemoService _creditMemoService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IOAuthService _oauthService;
        private readonly IEstimateService _estimateSrService;

        public ActionResult Save()
        {
            try
            {
                SaveData();
                ViewBag.IsCreated = true;
                return View("Index");
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
                RecalculateDocuments();
                ViewBag.IsRecalculated = true;
                return View("Index");
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to recalculate sales tax", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private void SaveData()
        {
            var context = _oauthService.GetServiceContext();
            var dataService = new DataService(context);
            var creditMemos = dataService.FindAll(new CreditMemo()).ToList();
            _creditMemoService.Save(creditMemos);
            var preferences = dataService.FindAll(new Preferences()).ToList();
            var accountingMethod = preferences[0].ReportPrefs.ReportBasis;
            var invoices = dataService.FindAll(new Invoice()).ToList();
            _invoiceService.Save(invoices, accountingMethod);
            var salesReceipts = dataService.FindAll(new SalesReceipt()).ToList();
            _salesReceiptService.Save(salesReceipts);
        }

        private void RecalculateDocuments()
        {
            var context = _oauthService.GetServiceContext();
            context.IppConfiguration.Message.Request.SerializationFormat =
                Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            context.IppConfiguration.Message.Response.SerializationFormat =
                Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            _invoiceService.Recalculate(context);
            _creditMemoService.Recalculate(context);
            _salesReceiptService.Recalculate(context);
            _estimateSrService.Recalculate(context);
        }
    }
}