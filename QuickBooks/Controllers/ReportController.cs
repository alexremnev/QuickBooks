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
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using QuickBooks.Models.EntityService;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILog _log = LogManager.GetLogger<ReportController>();
        private readonly ICreditMemoService _creditMemoService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IOAuthService _oauthService;
        public ReportController(ICreditMemoService creditMemoService, IInvoiceService invoiceService, ISalesReceiptService salesReceiptService, IOAuthService oauthService)
        {
            _creditMemoService = creditMemoService;
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _oauthService = oauthService;
        }

        public ActionResult Create()
        {
            try
            {
                var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
                var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
                var appToken = @"926acffab65abb4251b9d29b3d7daf2f4bf5";
                var intuitServicesType = IntuitServicesType.QBO;
                var permission = _oauthService.Get();
                OAuthRequestValidator oauthValidator = new OAuthRequestValidator(permission.AccessToken, permission.AccessTokenSecret, consumerKey, consumerSecret);
                ServiceContext context = new ServiceContext(appToken, permission.RealmId, intuitServicesType, oauthValidator);
                DataService dataService = new DataService(context);
                IList<CreditMemo> creditMemos = dataService.FindAll(new CreditMemo()).ToList();
                _creditMemoService.Save(creditMemos);
                IList<Invoice> invoices = dataService.FindAll(new Invoice()).ToList();
                _invoiceService.Save(invoices);
                IList<SalesReceipt> sales = dataService.FindAll(new SalesReceipt()).ToList();
                _salesReceiptService.Save(sales);
                return Json(invoices, JsonRequestBehavior.AllowGet);
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
                var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
                var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
                var appToken = @"926acffab65abb4251b9d29b3d7daf2f4bf5";
                var intuitServicesType = IntuitServicesType.QBO;
                var permission = _oauthService.Get();
                OAuthRequestValidator oauthValidator = new OAuthRequestValidator(permission.AccessToken, permission.AccessTokenSecret, consumerKey, consumerSecret);
                ServiceContext context = new ServiceContext(appToken, permission.RealmId, intuitServicesType, oauthValidator);
                context.IppConfiguration.Message.Request.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
                context.IppConfiguration.Message.Response.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
                DataService dataService = new DataService(context);

                var queryService = new QueryService<Estimate>(context);
                // var d = new decimal(362.07);
                var d = new decimal(70);


                var estimates = queryService.Where(e => e.TotalAmt == d).ToList();

                foreach (var estimate in estimates)
                {
                    TxnTaxDetail detail = new TxnTaxDetail();
                    detail.TxnTaxCodeRef = new ReferenceType()
                    {
                        name = "CA",
                        Value = 5.ToString()
                    };
                    detail.TaxLine[0] = new Line()
                    {
                        DetailTypeSpecified = true,
                        Amount = new decimal(14),
                        AmountSpecified = true,

                    };
                    detail.TotalTax = new decimal(14);
                    estimate.TxnTaxDetail = detail;
                    

                    //estimate.DocNumber = 1111.ToString();
                    dataService.Update(estimate);
                }

                //var queryService = new QueryService<Estimate>(context);
                //var estimates = queryService.Where(e => e.TotalAmt > 1000).ToList();

                //                var estimates = queryService.Where(e => e.ShipAddr.CountrySubDivisionCode == "CA").ToList();
                //
                //                foreach (var estimate in estimates)
                //                {
                //                    estimate.
                //                    if (estimate.ShipAddr.CountrySubDivisionCode == "CA")
                //                    {
                //                        if (estimate.TxnTaxDetail != null)
                //                        {
                //                            var detail = estimate.TxnTaxDetail.TaxLine[0].AnyIntuitObject;
                //                            if ((detail != null) && (detail is Intuit.Ipp.Data.TxnTaxDetail))
                //                            {
                //                                ((TxnTaxDetail)detail).TotalTax = 10;
                //                                dataService.Update(estimate);
                //                            }
                //                        }
                //                    }
                //
                //
                //                }


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