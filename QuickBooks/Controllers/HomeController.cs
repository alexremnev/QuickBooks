using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Common.Logging;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using QuickBooks.Models.Business;
using QuickBooks.Models.DAL;

namespace QuickBooks.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IOAuthService oAuthService, INotificationService notificationService)
        {
            _oauthService = oAuthService;
            _notificationService = notificationService;
        }
        private readonly IOAuthService _oauthService;
        private readonly INotificationService _notificationService;
        private static readonly ILog Log = LogManager.GetLogger<HomeController>();
        private static readonly string RequestTokenUrl = ConfigurationManager.AppSettings["GetRequestToken"];
        private static readonly string AccessTokenUrl = ConfigurationManager.AppSettings["GetAccessToken"];
        private static readonly string AuthorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        private static readonly string OauthUrl = ConfigurationManager.AppSettings["OauthLink"];
        private static readonly string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private static readonly string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
        private readonly State state = new State();

        public ActionResult Index()
        {
            try
            {
                var permission = _oauthService.Get();
                if (permission?.AccessToken == null) return View(state);
                state.realmId = permission.RealmId;
                string notifications = null;
                object hmacHeaderSignature = null;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    notifications = new StreamReader(Request.InputStream).ReadToEnd();
                    hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
                }
                var isRequestValid = _notificationService.VerifyPayload(hmacHeaderSignature?.ToString(), notifications);
                if (!isRequestValid) return View(state);
                //                Task.Factory.StartNew(()=> _notificationService.Process(notifications));
                _notificationService.Process(notifications);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
                //return View(state);
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to get permissions", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Oauth([FromUri] bool? connect)
        {
            try
            {
                if (connect != null && connect == true) FireAuth();
                return RedirectToAction("Index", state);
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to get request token into Quickbooks", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult GetOauth([FromUri] string oauth_token, [FromUri] string oauth_verifier,
            [FromUri] string realmId)
        {
            try
            {
                if (oauth_token == null || oauth_verifier == null || realmId == null)
                    RedirectToAction("Index", state);
                GetAndSaveAccessToken(oauth_token, oauth_verifier, realmId);
                state.realmId = realmId;
                return View("Close");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to get access token into Quickbooks", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private static IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext, RequestTokenUrl, OauthUrl, AccessTokenUrl);
        }

        private void GetAndSaveAccessToken(string oauthToken, string verifier, string realmId)
        {
            var token = (string)Session["tokenSecret"];
            var clientSession = CreateSession();
            var requestTokenSecret = HttpContext.Request.Cookies["requestTokenSecret"]?.Value;
            var requesToken = new RequestToken
            {
                ConsumerKey = ConsumerKey,
                Token = oauthToken,
                TokenSecret = requestTokenSecret
            };
            var accessToken = clientSession.ExchangeRequestTokenForAccessToken(requesToken, verifier);
            var oAuth = new OAuth
            {
                AccessToken = accessToken.Token,
                AccessTokenSecret = accessToken.TokenSecret,
                RealmId = realmId
            };
            _oauthService.Delete();
            _oauthService.Save(oAuth);
            RedirectToActionPermanent("Index", "Home");
        }

        private void FireAuth()
        {
            var session = CreateSession();
            var requestToken = session.GetRequestToken();
            var cookie = new HttpCookie("requestTokenSecret")
            {
                Value = requestToken.TokenSecret,
                Expires = DateTime.Now.AddMinutes(10)
            };
            HttpContext.Response.SetCookie(cookie);
            Session["tokenSecret"] = requestToken.TokenSecret;
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(BaseUrl)}/GetOauth";
            Response.Redirect(authUrl);
        }

        //public ActionResult Notification()
        //{
        //    try
        //    {
        //        string notifications = null;
        //        object hmacHeaderSignature = null;
        //        if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
        //        {
        //            System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
        //            notifications = new StreamReader(Request.InputStream).ReadToEnd();
        //            hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
        //        }
        //        var isRequestValid = _notificationService.VerifyPayload(hmacHeaderSignature?.ToString(),
        //            notifications);
        //        if (!isRequestValid) return View("Index");
        //        _notificationService.Process(notifications);
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error("Exception occured when application tried to handle incoming notifications", e);
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        //    }
        //}
    }
}