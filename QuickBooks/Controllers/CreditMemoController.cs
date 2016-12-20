using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Security;
using QuickBooks.Models.Services;

namespace QuickBooks.Controllers
{
    public class CreditMemoController : Controller
    {
        private readonly ICreditMemoService _service;
        public CreditMemoController(ICreditMemoService service)
        {
            _service = service;
        }
       // GET: CreditMemo
        public ActionResult Pull()
        {
            var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            var appToken = @"926acffab65abb4251b9d29b3d7daf2f4bf5";

            string realmId = 123145730707769.ToString();
            var intuitServicesType = IntuitServicesType.QBO;
            OAuthRequestValidator oauthValidator = new OAuthRequestValidator(System.Web.HttpContext.Current.Session["accessToken"].ToString(), System.Web.HttpContext.Current.Session["accessTokenSecret"].ToString(), consumerKey, consumerSecret);
            ServiceContext context = new ServiceContext(appToken, realmId, intuitServicesType, oauthValidator);
            DataService dataService = new DataService(context);
            IList<CreditMemo> creditMemos = dataService.FindAll(new CreditMemo()).ToList();
            _service.Save(creditMemos);
            return Json(creditMemos, JsonRequestBehavior.AllowGet);
        }
    }
}