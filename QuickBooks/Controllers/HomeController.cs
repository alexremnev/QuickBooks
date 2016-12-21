using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using Intuit.Ipp.Data;

namespace QuickBooks.Controllers
{
    public class HomeController : Controller
    {
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
            IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken(_requesToken, _verifier);
            System.Web.HttpContext.Current.Session["accessToken"] = accessToken.Token;
            System.Web.HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;
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
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(OauthCallbackUrl)}";
            System.Web.HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
           
        }
    }
}