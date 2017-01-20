using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Mvc;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Newtonsoft.Json;
using QuickBooks.Models.DAL;
using QuickBooks.Models.EntityService;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Utility;

namespace QuickBooks.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOAuthService _oauthService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISalesReceiptService _salesReceiptService;
        private readonly IEstimateService _estimateService;
        private readonly ICreditMemoService _creditMemoService;
        private readonly IReportService _reportService;

        public HomeController(IOAuthService oAuthService, IInvoiceService invoiceService, ISalesReceiptService salesReceiptService, IEstimateService estimateService, ICreditMemoService creditMemoService, IReportService reportService)
        {
            _oauthService = oAuthService;
            _invoiceService = invoiceService;
            _salesReceiptService = salesReceiptService;
            _estimateService = estimateService;
            _creditMemoService = creditMemoService;
            _reportService = reportService;
        }
        private static readonly string RequestTokenUrl = ConfigurationManager.AppSettings["GET_REQUEST_TOKEN"];
        private static readonly string AccessTokenUrl = ConfigurationManager.AppSettings["GET_ACCESS_TOKEN"];
        private static readonly string AuthorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        private static readonly string OauthUrl = ConfigurationManager.AppSettings["OauthLink"];
        private static readonly string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private static readonly string OauthCallbackUrl = "http://localhost:63793/Home/Result";
        private static string _verifier;
        private static IToken _requesToken;

        public ActionResult Index()
        {
            var realmId = ConfigurationManager.AppSettings["realmId"];
            var permission = _oauthService.Get(realmId);
            if (permission?.AccessToken != null) ViewBag.Access = true;
            string notifications = null;
            object hmacHeaderSignature = null;
            if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
            {
                System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                notifications = new StreamReader(Request.InputStream).ReadToEnd();
                hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
            }
            var isRequestvalid = ProcessNotificationData.Validate(notifications, hmacHeaderSignature);
            if (isRequestvalid)
            {
                var webhooksData = JsonConvert.DeserializeObject<NotificationEntity.WebhooksData>(notifications);
                var context = _oauthService.GetServiceContext();
                foreach (var notification in webhooksData.EventNotifications)
                {
                    foreach (var entity in notification.DataEvents.Entities)
                    {
                        if (entity.Operation == "Delete")
                        {
                            _reportService.Delete(entity.Id);
                            continue;
                        }

                        switch (entity.Name)
                        {
                            case "Invoice":
                                _invoiceService.Update(context, entity);
                                break;
                            case "CreditMemo":
                                _creditMemoService.Update(context, entity);
                                break;
                            case "Estimate":
                                _estimateService.Update(context, entity);
                                break;
                            case "SalesReceipt":
                                _salesReceiptService.Update(context, entity);
                                break;
                        }
                    }
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return View();
        }

        public ActionResult Result()
        {
            if (Request.QueryString.Count > 0)
            {
                var queryKeys = new List<string>(Request.QueryString.AllKeys);
                if (queryKeys.Contains("connect"))
                {
                    FireAuth();
                }
                if (queryKeys.Contains("oauth_token"))
                {
                    ReadToken();
                    var oAuth = new OAuth()
                    {
                        AccessToken = System.Web.HttpContext.Current.Session["accessToken"].ToString(),
                        AccessTokenSecret = System.Web.HttpContext.Current.Session["accessTokenSecret"].ToString(),
                        RealmId = Convert.ToInt64(System.Web.HttpContext.Current.Session["realm"]).ToString()
                    };
                    _oauthService.Save(oAuth);
                    ViewBag.Access = true;
                    return View("Close");
                }
            }
            return RedirectToAction("Index");

        }
        private void FireAuth()
        {
            System.Web.HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            System.Web.HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            CreateAuthorization();
        }

        private void ReadToken()
        {
            System.Web.HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"];
            System.Web.HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"];
            _verifier = Request.QueryString["oauth_verifier"];
            System.Web.HttpContext.Current.Session["realm"] = Request.QueryString["realmId"];
            System.Web.HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"];
            GetAccessToken();
        }
        protected IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1
            };

            return new OAuthSession(consumerContext,
                                    RequestTokenUrl,
                                    OauthUrl,
                                    AccessTokenUrl);
        }

        private void GetAccessToken()
        {
            IOAuthSession clientSession = CreateSession();
            var oauth = _oauthService.Get(ConsumerKey);
            _requesToken = new RequestToken { ConsumerKey = ConsumerKey, Token = oauth.AccessToken, TokenSecret = oauth.AccessTokenSecret };
            var accessToken = clientSession.ExchangeRequestTokenForAccessToken(_requesToken, _verifier);
            System.Web.HttpContext.Current.Session["accessToken"] = accessToken.Token;
            System.Web.HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;
            _oauthService.Delete(ConsumerKey);
        }

        protected void CreateAuthorization()
        {
            System.Web.HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            System.Web.HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            System.Web.HttpContext.Current.Session["oauthLink"] = OauthUrl;

            IOAuthSession session = CreateSession();
            IToken requestToken = session.GetRequestToken();
            System.Web.HttpContext.Current.Session["requestToken"] = requestToken;
            _requesToken = requestToken;
            var entity = new OAuth { RealmId = ConsumerKey, AccessToken = _requesToken.Token, AccessTokenSecret = _requesToken.TokenSecret };
            _oauthService.Save(entity);
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(OauthCallbackUrl)}";
            System.Web.HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }
    }
}