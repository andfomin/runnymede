using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

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

            CustomizeExceptionHandling(config);
            CustomizeConfiguration(config);
        }

        /* The "system.web/customErrors" section in Web.config affects both MVC and WebApi. 
         * If we set customErrors mode="On" and disble detailed info in MVC, we cannot pass an exception message in WebApi.
         * If we set customErrors mode="Off" and enable full info, the default error handler in WebApi passes the full info about the exception including the call stack and method names.
         * We need a custom error message handler in WebApi to pass our custom error message.
         */
        private static void CustomizeExceptionHandling(HttpConfiguration config)
        {
            // register the exception logger and handler
            //config.Services.Add(typeof(IExceptionLogger), new GlobalExceptionLogger());
            config.Services.Replace(typeof(IExceptionHandler), new CustomExceptionHandler());

            // Set the error detail policy according to the value in Web.config
            var customErrors = (CustomErrorsSection)ConfigurationManager.GetSection("system.web/customErrors");
            if (customErrors != null)
            {
                switch (customErrors.Mode)
                {
                    case CustomErrorsMode.RemoteOnly:
                        {
                            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                            break;
                        }
                    case CustomErrorsMode.On:
                        {
                            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
                            break;
                        }
                    case CustomErrorsMode.Off:
                        {
                            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                            break;
                        }
                    default:
                        {
                            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
                            break;
                        }
                }
            }
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
