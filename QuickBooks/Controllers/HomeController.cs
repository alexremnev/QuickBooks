using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common.Logging;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using QuickBooks.Models.Business;
using QuickBooks.Models.Data;

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
        private static readonly string RequestTokenUrl = ConfigurationManager.AppSettings["qb.getRequestToken"];
        private static readonly string AccessTokenUrl = ConfigurationManager.AppSettings["qb.getAccessToken"];
        private static readonly string AuthorizeUrl = ConfigurationManager.AppSettings["qb.authorizeUrl"];
        private static readonly string OauthUrl = ConfigurationManager.AppSettings["qb_oauthLink"];
        private static readonly string ConsumerKey = ConfigurationManager.AppSettings["qb.consumerKey"];
        private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["qb.consumerSecret"];
        private static readonly string BaseUrl = ConfigurationManager.AppSettings["baseUrl"];
        private readonly State state = new State();

        public ActionResult Index()
        {
            try
            {
                state.realmIds = _oauthService.List().Select(x => x.RealmId).ToList();
                return View("Index", state);
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
                return FireAuth();
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to get request token into Quickbooks", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult GetOauth(string oauth_verifier, string realmId)
        {
            try
            {
                if (oauth_verifier == null || realmId == null)
                    RedirectToAction("Index", state);
                GetAndSaveAccessToken(oauth_verifier, realmId);
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

        private void GetAndSaveAccessToken(string verifier, string realmId)
        {
            var clientSession = CreateSession();
            var requestToken = (IToken) Session["token"];
            var accessToken = clientSession.ExchangeRequestTokenForAccessToken(requestToken, verifier);
            var oAuth = new OAuth
            {
                AccessToken = accessToken.Token,
                AccessTokenSecret = accessToken.TokenSecret,
                RealmId = realmId
            };
            _oauthService.Delete(realmId);
            _oauthService.Save(oAuth);
            RedirectToActionPermanent("Index", "Home");
        }

        private ActionResult FireAuth()
        {
            var session = CreateSession();
            var requestToken = session.GetRequestToken();
            Session["token"] = requestToken;
            var authUrl =
                $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(BaseUrl)}/GetOauth";
            return Redirect(authUrl);
        }

        public ActionResult Notification()
        {
            try
            {
                string notifications = null;
                object hmacHeaderSignature = null;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    notifications = new StreamReader(Request.InputStream).ReadToEnd();
                    hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
                }
                var isRequestValid = _notificationService.VerifyPayload(hmacHeaderSignature?.ToString(), notifications);
                if (!isRequestValid) return View("Index");
                Task.Factory.StartNew(() => _notificationService.Process(notifications));
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to handle incoming notifications", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
    }
}