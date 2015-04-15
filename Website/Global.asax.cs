using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Optimization;

namespace Runnymede.Website
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Remove the unused WebForms (ASPX) view engine.
            ViewEngines.Engines.Remove(ViewEngines.Engines.OfType<WebFormViewEngine>().FirstOrDefault());

            Runnymede.Website.Utils.AzureStorageUtils.EnsureStorageObjectsExist();

            // Suppresses "X-AspNetMvc-Version",  +http://blog.paulbouwer.com/2013/01/09/asafaweb-excessive-headers-and-windows-azure/
            // We do it the other way, see Application_PreSendRequestHeaders()
            //MvcHandler.DisableMvcResponseHeader = true; 
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                //HttpContext.Current.Response.Headers.Remove("X-Powered-By"); // Done in Web.config <system.webServer><httpProtocol><customHeaders><remove name="X-Powered-By"/>
                //HttpContext.Current.Response.Headers.Remove("X-AspNet-Version"); // Done in Web.config <system.web><httpRuntime enableVersionHeader="false" />
                context.Response.Headers.Remove("X-AspNetMvc-Version");
                /* Web.config might work on Azure, it does not work on the dev machine. <system.webServer><security><requestFiltering removeServerHeader="true" />
                 * Config Error on localhost: Unrecognized attribute 'removeServerHeader'
                 * +http://blogs.msdn.com/b/windowsazure/archive/2013/11/22/removing-standard-server-headers-on-windows-azure-web-sites.aspx . */
                context.Response.Headers.Remove("Server");
                // X-SourceFiles is generated only for localhost
            }
        }
    }
}