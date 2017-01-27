using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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
        public HomeController(IOAuthService oAuthService, IProcessNotificationData processNotificationData)
        {
            _oauthService = oAuthService;
            _processNotificationData = processNotificationData;
        }
        private readonly IOAuthService _oauthService;
        private readonly IProcessNotificationData _processNotificationData;
        private static readonly ILog Log = LogManager.GetLogger<HomeController>();
        private static readonly string RequestTokenUrl = ConfigurationManager.AppSettings["GET_REQUEST_TOKEN"];
        private static readonly string AccessTokenUrl = ConfigurationManager.AppSettings["GET_ACCESS_TOKEN"];
        private static readonly string AuthorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        private static readonly string OauthUrl = ConfigurationManager.AppSettings["OauthLink"];
        private static readonly string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private static readonly string OauthCallbackUrl = ConfigurationManager.AppSettings["OauthCallbackUrl"];
        private static readonly string RealmId = ConfigurationManager.AppSettings["realmId"];

        public ActionResult Index()
        {
            try
            {
                var permission = _oauthService.Get(RealmId);
                if (permission?.AccessToken == null) return View();
                ViewBag.Access = true;
                string notifications = null;
                object hmacHeaderSignature = null;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    notifications = new StreamReader(Request.InputStream).ReadToEnd();
                    hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
                }
                var isRequestValid = _processNotificationData.VerifyPayload(hmacHeaderSignature?.ToString(), notifications);
                if (!isRequestValid) return View();
                _processNotificationData.Update(notifications, _oauthService);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
                //return View();
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to get permissions", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Oauth()
        {
            try
            {
                if (Request.QueryString.Count <= 0) return RedirectToAction("Index");
                if (Request.QueryString.AllKeys.Contains("connect"))
                {
                    FireAuth();
                }
                if (!Request.QueryString.AllKeys.Contains("oauth_token")) return RedirectToAction("Index");
                GetAndSaveAccessToken(Request.QueryString["oauth_verifier"]);
                ViewBag.Access = true;
                return View("Close");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to get access into Quickbooks", e);
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

        private void GetAndSaveAccessToken(string verifier)
        {
            var clientSession = CreateSession();
            var oauthToken = Request.QueryString["oauth_token"];
            var oauth = _oauthService.Get(oauthToken);
            if (oauth != null)
            {
                var requesToken = new RequestToken
                {
                    ConsumerKey = ConsumerKey,
                    Token = oauth.AccessToken,
                    TokenSecret = oauth.AccessTokenSecret
                };
                var accessToken = clientSession.ExchangeRequestTokenForAccessToken(requesToken, verifier);
                var oAuth = new OAuth
                {
                    AccessToken = accessToken.Token,
                    AccessTokenSecret = accessToken.TokenSecret,
                    RealmId = Request.QueryString["realmId"]
                };
                _oauthService.Delete(oauthToken);
                _oauthService.Save(oAuth);
            }
            RedirectToAction("Index");
        }

        private void FireAuth()
        {
            var session = CreateSession();
            var requestToken = session.GetRequestToken();
            var entity = new OAuth { RealmId = requestToken.Token, AccessToken = requestToken.Token, AccessTokenSecret = requestToken.TokenSecret };
            _oauthService.Save(entity);
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(OauthCallbackUrl)}";
            Response.Redirect(authUrl);
        }

//        public ActionResult Notification()
//        {
//            try
//            {
//                string notifications = null;
//                object hmacHeaderSignature = null;
//                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
//                {
//                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
//                    notifications = new StreamReader(Request.InputStream).ReadToEnd();
//                    hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
//                }
//                var isRequestValid = _processNotificationData.VerifyPayload(hmacHeaderSignature?.ToString(),
//                    notifications);
//                if (!isRequestValid) return View("Index");
//                _processNotificationData.Update(notifications, _oauthService);
//                return new HttpStatusCodeResult(HttpStatusCode.OK);
//            }
//            catch (Exception e)
//            {
//                Log.Error("Exception occured when application tried to handle incoming notifications", e);
//                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
//            }
//        }
    }
}