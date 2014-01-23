using Microsoft.Owin.Security.OAuth;
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
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Use camel case for JSON data.
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            Configure(config);
        }
        
        private static void Configure(HttpConfiguration config)
        {
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code. For more information, refer to: +http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();

            var json = config.Formatters.JsonFormatter;
            // +http://www.asp.net/web-api/overview/formats-and-model-binding/json-and-xml-serialization
            // Here we configure it to write JSON property names with camel casing without changing our server-side data model:
            // Works by default. json.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            //json.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
        }

    }
}
