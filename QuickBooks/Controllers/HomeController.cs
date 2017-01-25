using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to recalculate document", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Result()
        {
            try
            {
                if (Request.QueryString.Count <= 0) return RedirectToAction("Index");
                var queryKeys = new List<string>(Request.QueryString.AllKeys);
                if (queryKeys.Contains("connect"))
                {
                    FireAuth();
                }
                if (!queryKeys.Contains("oauth_token")) return RedirectToAction("Index");
                ReadToken();
                var oAuth = new OAuth()
                {
                    AccessToken = System.Web.HttpContext.Current.Session["accessToken"].ToString(),
                    AccessTokenSecret = System.Web.HttpContext.Current.Session["accessTokenSecret"].ToString(),
                    RealmId = Convert.ToInt64(System.Web.HttpContext.Current.Session["realm"]).ToString()
                };
                _oauthService.Delete(oAuth.RealmId);
                _oauthService.Save(oAuth);
                ViewBag.Access = true;
                return View("Close");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to get access into Quickbooks", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private void ReadToken()
        {
            System.Web.HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"];
            System.Web.HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"];
            var verifier = Request.QueryString["oauth_verifier"];
            System.Web.HttpContext.Current.Session["realm"] = Request.QueryString["realmId"];
            System.Web.HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"];
            GetAccessToken(verifier);
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

        private void GetAccessToken(string verifier)
        {
            var clientSession = CreateSession();
            var oauth = _oauthService.Get(ConsumerKey);
            var requesToken = new RequestToken { ConsumerKey = ConsumerKey, Token = oauth.AccessToken, TokenSecret = oauth.AccessTokenSecret };
            var accessToken = clientSession.ExchangeRequestTokenForAccessToken(requesToken, verifier);
            System.Web.HttpContext.Current.Session["accessToken"] = accessToken.Token;
            System.Web.HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;
            _oauthService.Delete(ConsumerKey);
        }

        private void FireAuth()
        {
            System.Web.HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            System.Web.HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            System.Web.HttpContext.Current.Session["oauthLink"] = OauthUrl;
            var session = CreateSession();
            var requestToken = session.GetRequestToken();
            System.Web.HttpContext.Current.Session["requestToken"] = requestToken;
            var entity = new OAuth { RealmId = ConsumerKey, AccessToken = requestToken.Token, AccessTokenSecret = requestToken.TokenSecret };
            _oauthService.Save(entity);
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(OauthCallbackUrl)}";
            System.Web.HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }
    }
}