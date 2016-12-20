using System;
using System.Configuration;
using System.Web;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.Collections.Generic;

namespace IDGOauthSample
{
    public partial class OauthManager : System.Web.UI.Page
    {
        public static string RequestTokenUrl = ConfigurationManager.AppSettings["GET_REQUEST_TOKEN"];
        public static string AccessTokenUrl = ConfigurationManager.AppSettings["GET_ACCESS_TOKEN"];
        public static string AuthorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        public static string OauthUrl = ConfigurationManager.AppSettings["OauthLink"];
        public string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string StrrequestToken = string.Empty;
        public string TokenSecret = string.Empty;
        public string OauthCallbackUrl = "http://localhost:65281/OauthManager.aspx?";
        public string GrantUrl = "http://localhost:65281/OauthManager.aspx?connect=true";

        protected void Page_Load(object sender, EventArgs e)
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
            else
            {
                if (HttpContext.Current.Session["accessToken"] == null && HttpContext.Current.Session["accessTokenSecret"] == null)
                {
                    c2qb.Visible = true;
                    disconnect.Visible = false;
                    lblDisconnect.Visible = false;
                }
                else
                {
                    c2qb.Visible = false;
                    disconnect.Visible = true;
                    //Disconnect();
                }
            }
        }
        /// <summary>
        /// Initiate the ouath screen.
        /// </summary>
        private void FireAuth()
        {
            HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            CreateAuthorization();
            var token = (IToken)HttpContext.Current.Session["requestToken"];
            TokenSecret = token.TokenSecret;
            StrrequestToken = token.Token;
        }
        /// <summary>
        /// Read the values from the query string.
        /// </summary>
        private void ReadToken()
        {
            HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"];
            HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"];
            HttpContext.Current.Session["realm"] = Request.QueryString["realmId"];
            HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"];
            //Stored in a session for demo purposes.
            //Production applications should securely store the Access Token
            getAccessToken();
        }



        /// <summary>
        /// Create a session.
        /// </summary>
        /// <returns></returns>
        protected IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = HttpContext.Current.Session["consumerKey"].ToString(),
                ConsumerSecret = HttpContext.Current.Session["consumerSecret"].ToString(),
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext,
                                    RequestTokenUrl,
                                    HttpContext.Current.Session["oauthLink"].ToString(),
                                    AccessTokenUrl);
        }
        /// <summary>
        /// Get Access token.
        /// </summary>
        private void getAccessToken()
        {
            IOAuthSession clientSession = CreateSession();
            IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken((IToken)HttpContext.Current.Session["requestToken"], HttpContext.Current.Session["oauthVerifyer"].ToString());
            HttpContext.Current.Session["accessToken"] = accessToken.Token;
            HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;
        }

        protected void CreateAuthorization()
        {
            HttpContext.Current.Session["consumerKey"] = ConsumerKey;
            HttpContext.Current.Session["consumerSecret"] = ConsumerSecret;
            HttpContext.Current.Session["oauthLink"] = OauthUrl;

            IOAuthSession session = CreateSession();
            IToken requestToken = session.GetRequestToken();
            HttpContext.Current.Session["requestToken"] = requestToken;
            TokenSecret = requestToken.TokenSecret;
            var authUrl = string.Format("{0}?oauth_token={1}&oauth_callback={2}", AuthorizeUrl, requestToken.Token, UriUtility.UrlEncode(OauthCallbackUrl));
            HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }


        protected void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                Session.Clear();
                Session.Abandon();
                HttpContext.Current.Session["accessToken"] = null;
                HttpContext.Current.Session["accessTokenSecret"] = null;
                HttpContext.Current.Session["realm"] = null;
                HttpContext.Current.Session["dataSource"] = null;
                disconnect.Visible = false;
                lblDisconnect.Visible = true;
            }
            catch (Exception ex)
            {
                Response.Write(ex.InnerException);
            }
        }
    }
}