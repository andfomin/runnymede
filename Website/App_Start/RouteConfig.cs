using LowercaseDashedRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Runnymede.Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            //routes.LowercaseUrls = true; We use +https://www.nuget.org/packages/LowercaseDashedRoute/ instead.
            var route = new LowercaseDashedRoute(
                    "{controller}/{action}/{id}",
                    new RouteValueDictionary(new { controller = "Home", action = "Index", id = UrlParameter.Optional }),
                    null,
                    new RouteValueDictionary(new { namespaces = new string[] { "Runnymede.Website.Controllers" } }),
                    new DashedRouteHandler()
                );
            routes.Add("Default", route);

        }
    }
}
