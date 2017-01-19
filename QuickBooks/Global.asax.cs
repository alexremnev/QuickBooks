using System.Web.Mvc;
using System.Web.Routing;
using Spring.Web.Mvc;

namespace QuickBooks
{
    public class MvcApplication : SpringMvcApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
       }
    }
}
