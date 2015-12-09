using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Runnymede.Website.Utils
{
    public class CustomController : Controller
    {
        #region Override to reroute to non-SSL port if controller action does not have RequireHttps attribute to save on CPU
        // By L. Keng, 2012/08/27
        // Note that this code works with RequireHttps at the controller class or action level.
        // Credit: Various stackoverflow.com posts and +http://puredotnetcoder.blogspot.com/2011/09/requirehttps-attribute-in-mvc3.html
        // To achive same functionality for WebAPI ApiController see +http://codebetter.com/johnvpetersen/2012/04/02/making-your-asp-net-web-apis-secure/
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If the controller class or the action has a RequireHttps attribute
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
                        Port = 80, 
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
                        Port = 443,
                    };
                    filterContext.Result = this.Redirect(uriBuilder.Uri.AbsoluteUri);
                }
            }

            base.OnAuthorization(filterContext);
        }
        #endregion


        //public void ChangeCookie(string name, string value, bool active, DateTime? expirationTime = null)
        //{
        //    /* System.Web.Security.FormsAuthentication.Encript  System.Web.Security.FormsAuthenticationTicket */
        //    var exists = Request.Cookies.AllKeys.Contains(name);
        //    var cookie = String.IsNullOrEmpty(value) ? new HttpCookie(name) : new HttpCookie(name, value);

        //    if (active)
        //    {
        //        // A cookie with no expiration time set is a session cookie.
        //        if (expirationTime.HasValue)
        //        {
        //            var expTime = expirationTime.Value;
        //            var maxTime = new DateTime(2038, 01, 01); // Unix Millennium Bug. 19 January 2038 03:14:07 UTC 
        //            cookie.Expires = expTime < maxTime ? expTime : maxTime;
        //        }

        //        if (exists)
        //        {
        //            Response.Cookies.Set(cookie);
        //        }
        //        else
        //        {
        //            Response.Cookies.Add(cookie);
        //        }
        //    }
        //    else
        //    {
        //        if (exists)
        //        {
        //            cookie.Expires = new DateTime(1999, 10, 12, 04, 00, 00, DateTimeKind.Utc);// A "magic" number in ASP.NET :) +http://stackoverflow.com/questions/701030/whats-the-significance-of-oct-12-1999
        //            Response.Cookies.Set(cookie);
        //        }
        //    }
        //}

        ////protected string GetControllerName()
        ////{
        ////    return this.RouteData.GetRequiredString("controller");
        ////}

    }
}