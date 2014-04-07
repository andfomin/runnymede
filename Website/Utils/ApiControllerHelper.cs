using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using Owin;
using System.Security.Principal;

namespace Runnymede.Website.Utils
{
    public static class ApiControllerHelper
    {
        public static int GetUserId(this ApiController controller)
        {
            return IdentityHelper.GetUserId(controller.RequestContext.Principal.Identity);
        }

        public static string GetUserName(this ApiController controller)
        {
            return IdentityHelper.GetUserName(controller.RequestContext.Principal.Identity);
        }

        public static string GetUserDisplayName(this ApiController controller)
        {
            return IdentityHelper.GetUserDisplayName(controller.RequestContext.Principal.Identity);
        }

        public static bool GetUserIsTeacher(this ApiController controller)
        {
            return IdentityHelper.GetUserIsTeacher(controller.RequestContext.Principal.Identity);
        }

        public static string GetKeeper(this ApiController controller)
        {
            var cookies = controller.Request.Headers.GetCookies(LoggingUtils.KeeperCookieName).FirstOrDefault();
            var cookie = cookies != null ? cookies.Cookies.FirstOrDefault() : null;
            return cookie != null ? cookie.Value : Guid.Empty.ToString("N").ToUpper();
        }

    }
}