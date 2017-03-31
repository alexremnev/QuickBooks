using System.Web.Mvc;
using System.Web.Routing;

namespace QuickBooks
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
//            routes.MapRoute("A", "{controller}/{action}{oauth_token}{oauth_verifier}{realmId}",
//               new { controller = "Home", action = "GetOauth" },
//               new { oauth_token = @"\w*oken$", oauth_verifier = @"\w*erifier$", realmId = @"\w" }
//           );
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
            );
           
        }
    }
}