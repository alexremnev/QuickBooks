using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;

namespace QuickBooks.Controllers
{
    public class HomeController : Controller
    {
        public static string RequestTokenUrl = ConfigurationManager.AppSettings["GET_REQUEST_TOKEN"];
        public static string AccessTokenUrl = ConfigurationManager.AppSettings["GET_ACCESS_TOKEN"];
        public static string AuthorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        public static string OauthUrl = ConfigurationManager.AppSettings["OauthLink"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string StrrequestToken = string.Empty;
        public string TokenSecret = string.Empty;
        public string OauthCallbackUrl = "http://localhost:63793/Home/Result";
        public string GrantUrl = "http://localhost:63793/Home/Result?connect=true";

        private static string verifier;
        private static IToken requesToken;

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
                }
            }
            return RedirectToAction("Index");

        }
        private void FireAuth()
        {
            System.Web.HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            System.Web.HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            CreateAuthorization();
            var token = (IToken)System.Web.HttpContext.Current.Session["requestToken"];
            TokenSecret = token.TokenSecret;
            StrrequestToken = token.Token;
        }
        /// <summary>
        /// Read the values from the query string.
        /// </summary>
        private void ReadToken()
        {
            System.Web.HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"];
            System.Web.HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"];
            verifier = Request.QueryString["oauth_verifier"];
            System.Web.HttpContext.Current.Session["realm"] = Request.QueryString["realmId"];
            System.Web.HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"];
            //Stored in a session for demo purposes.
            //Production applications should securely store the Access Token
            GetAccessToken();
        }
        protected IOAuthSession CreateSession()
        {
         var consumerContext = new OAuthConsumerContext();

            consumerContext.ConsumerKey = ConsumerKey;
            consumerContext.ConsumerSecret = ConsumerSecret;
            consumerContext.SignatureMethod = SignatureMethod.HmacSha1;
            

            //return new OAuthSession(consumerContext,
            //                        RequestTokenUrl,
            //                        System.Web.HttpContext.Current.Session["oauthLink"].ToString(),
            //                        AccessTokenUrl);
            return new OAuthSession(consumerContext,
                                    RequestTokenUrl,
                                    OauthUrl,
                                    AccessTokenUrl);
        }
        /// <summary>
        /// Get Access token.
        /// </summary>
        private void GetAccessToken()
        {
            IOAuthSession clientSession = CreateSession();
           // IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken((IToken)System.Web.HttpContext.Current.Session["requestToken"], System.Web.HttpContext.Current.Session["oauthVerifyer"].ToString());
            IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken(requesToken, verifier);
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
            requesToken = requestToken;
            TokenSecret = requestToken.TokenSecret;
            var authUrl = $"{AuthorizeUrl}?oauth_token={requestToken.Token}&oauth_callback={UriUtility.UrlEncode(OauthCallbackUrl)}";
            System.Web.HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }

        public static string AccessToken()
        {
            return System.Web.HttpContext.Current.Session["accessToken"].ToString();
        }
        public static string AccessTokenSecret()
        {
            return System.Web.HttpContext.Current.Session["accessToken"].ToString();
        }
    }
}