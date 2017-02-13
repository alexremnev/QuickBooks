using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Common.Logging;
using QuickBooks.Models.Business;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        public ReportController(ICreditMemoService creditMemoService, IInvoiceService invoiceService,
           ISalesReceiptService salesReceiptService, params IService[] services)
        {
            _creditMemoService = creditMemoService;
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _services = services;
        }
        private static readonly ILog Log = LogManager.GetLogger<ReportController>();
        private readonly ICreditMemoService _creditMemoService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IList<IService> _services;

        public ActionResult Save()
        {
            try
            {
                SaveData();
                return RedirectToActionPermanent("Index", "Home");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to pull entity", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Recalculate()
        {
            try
            {
                RecalculateDocuments();
                return RedirectToActionPermanent("Index", "Home");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to recalculate sales tax", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private void SaveData()
        {
            _invoiceService.Save();
            _creditMemoService.Save();
            _salesReceiptService.Save();
        }

        private void RecalculateDocuments()
        {
            foreach (var service in _services)
            {
                service.Recalculate();
            }
        }
    }
}