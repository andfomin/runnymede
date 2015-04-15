using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Runnymede.Website
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new HostAuthenticationFilter(DefaultAuthenticationTypes.ApplicationCookie));        

            // Attribute routing.
            config.MapHttpAttributeRoutes();

            // Convention-based routing.
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            CustomizeConfiguration(config);
        }
        
        private static void CustomizeConfiguration(HttpConfiguration config)
        {
            // To disable tracing in your application, please comment out or remove the following line of code. For more information, refer to: +http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();

            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            // +http://www.asp.net/web-api/overview/formats-and-model-binding/json-and-xml-serialization
            // Here we configure it to write JSON property names with camel casing without changing our server-side data model:
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            jsonSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }

    }
}
