using ReliableCDMS.App_Start;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ReliableCDMS
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // On application startup
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Try cleanup orphaned files
            try
            {
                var cleanupResult = Helpers.CleanupHelper.RemoveOrphanedFiles();
                Debug.WriteLine($"Cleanup Result: Success={cleanupResult.Success}, Message={cleanupResult.Message}," +
                    $" FilesDeleted={cleanupResult.FilesDeleted}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup cleanup failed: {ex.Message}");
            }
        }
    }
}