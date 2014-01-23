using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using Owin;

namespace Runnymede.Website.Utils
{
    public static class ApiControllerHelper
    {
        public static int GetUserId(this ApiController controller)
        {
            return Convert.ToInt32(controller.RequestContext.Principal.Identity.GetUserId());
        }

        public static string GetUserName(this ApiController controller)
        {
            return controller.RequestContext.Principal.Identity.GetUserName();
        }

        public static string GetDisplayName(this ApiController controller)
        {
            var identity = controller.RequestContext.Principal.Identity as System.Security.Claims.ClaimsIdentity;
            return identity != null ? identity.FindFirstValue(AppClaimTypes.DisplayName) : null; // FindFirstValue() returns null if not found.
        }

        public static string GetKeeper(this ApiController controller)
        {
            var cookies = controller.Request.Headers.GetCookies(LoggingUtils.KeeperCookieName).FirstOrDefault();
            var cookie = cookies != null ? cookies.Cookies.FirstOrDefault() : null;
            return cookie != null ? cookie.Value : Guid.Empty.ToString("N").ToUpper();
        }

    }
}