using System.Web.Mvc;
using System.Web.Routing;

namespace ReliableCDMS.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}"); // Ignore WebResource.axd and ScriptResource.axd requests
        }
    }
}