﻿using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Runnymede.Website.Utils
{
    public class CustomController : Controller
    {
        #region Override to reroute to non-SSL port if controller action does not have RequireHttps attribute to save on CPU
        // By L. Keng, 2012/08/27
        // Note that this code works with RequireHttps at the controller class or action level.
        // Credit: Various stackoverflow.com posts and +http://puredotnetcoder.blogspot.com/2011/09/requirehttps-attribute-in-mvc3.html
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // if the controller class or the action has RequireHttps attribute
            var requireHttps = (filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(RequireHttpsAttribute), true).Count() > 0
                                || filterContext.ActionDescriptor.GetCustomAttributes(typeof(RequireHttpsAttribute), true).Count() > 0);
            if (Request.IsSecureConnection)
            {
                // If request has a secure connection but we don't need SSL, and we are not on a child action   
                if (!requireHttps && !filterContext.IsChildAction)
                {
                    var uriBuilder = new UriBuilder(Request.Url)
                    {
                        Scheme = "http",
                        Port = int.Parse(GetAppSetting("HttpPort", "80")) // grab from config; default to port 80
                    };
                    filterContext.Result = this.Redirect(uriBuilder.Uri.AbsoluteUri);
                }
            }
            else
            {
                // If request does not have a secure connection but we need SSL, and we are not on a child action   
                if (requireHttps && !filterContext.IsChildAction)
                {
                    var uriBuilder = new UriBuilder(Request.Url)
                    {
                        Scheme = "https",
                        Port = int.Parse(GetAppSetting("HttpsPort", "443")) // grab from config; default to port 443
                    };
                    filterContext.Result = this.Redirect(uriBuilder.Uri.AbsoluteUri);
                }
            }
            base.OnAuthorization(filterContext);
        }
        #endregion

        // a useful helper function to get appSettings value; allow caller to specify a default value if one cannot be found
        /* 
In Web.Release.Config, add the following to clear out HttpPort and HttpsPort (to use the default 80 and 443).
<appSettings>
<add key="HttpPort" value="" xdt:Transform="SetAttributes" xdt:Locator="Match(key)"/>
<add key="HttpsPort" value="" xdt:Transform="SetAttributes" xdt:Locator="Match(key)"/>
</appSettings>                  
         */
        internal static string GetAppSetting(string name, string defaultValue = null)
        {
            var val = System.Configuration.ConfigurationManager.AppSettings[name];
            return (!string.IsNullOrWhiteSpace(val) ? val : defaultValue);
        }

        public void ChangeCookie(string name, string value, bool active, DateTime? expirationTime = null)
        {
            /* System.Web.Security.FormsAuthentication.Encript  System.Web.Security.FormsAuthenticationTicket */
            var exists = Request.Cookies.AllKeys.Contains(name);
            var cookie = string.IsNullOrEmpty(value) ? new HttpCookie(name) : new HttpCookie(name, value);

            if (active)
            {
                // A cookie with no expiration time set is a session cookie.
                if (expirationTime.HasValue)
                {
                    var expTime = expirationTime.Value;
                    var maxTime = new DateTime(2038, 01, 01); // Unix Millennium Bug. 19 January 2038 03:14:07 UTC 
                    cookie.Expires = expTime > maxTime ? expTime : maxTime;
                }

                if (exists)
                {
                    Response.Cookies.Set(cookie);
                }
                else
                {
                    Response.Cookies.Add(cookie);
                }
            }
            else
            {
                if (exists)
                {
                    cookie.Expires = new DateTime(1999, 10, 12, 04, 00, 00, DateTimeKind.Utc);// A "magic" number in ASP.NET :) +http://stackoverflow.com/questions/701030/whats-the-significance-of-oct-12-1999
                    Response.Cookies.Set(cookie);
                }
            }
        }

        public void EnsureKeeperCookie()
        {
            if (!Request.Cookies.AllKeys.Contains(LoggingUtils.KeeperCookieName))
            {
                // Set the Keeper cookie
                var value = Guid.NewGuid().ToString("N").ToUpper();
                var cookie = new HttpCookie(LoggingUtils.KeeperCookieName, value);
                cookie.Expires = DateTime.UtcNow.AddYears(10);
                Response.Cookies.Add(cookie);

                // Log the contact.
                var referer = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : null;
                var languages = Request.UserLanguages ?? Enumerable.Empty<string>();

                var logData =
                    new XElement("LogData",
                        new XElement("Kind", LoggingUtils.Kind.Referer),
                        new XElement("Time", DateTime.UtcNow),
                        new XElement("Keeper", value),
                        new XElement("RefererUrl", referer),
                        new XElement("Host", Request.UserHostAddress),
                        new XElement("UserAgent", Request.UserAgent),
                        new XElement("UserLanguages",
                            from language in languages
                            select new XElement("Language", language)
                        )
                    )
                    .ToString(SaveOptions.DisableFormatting);

                LoggingUtils.WriteKeeperLog(logData);
            }
        }

        protected string GetControllerName()
        {
            return this.RouteData.GetRequiredString("controller");
        }


    }
}