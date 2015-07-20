using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Runnymede.Common.Utils;

namespace Runnymede.Website.Controllers.Mvc
{
    public class HomeController : Runnymede.Website.Utils.CustomController
    {
        public async Task<ActionResult> Index()
        {
            await this.EnsureExtIdCookieAsync();

            return View();

            //if (Request.IsAuthenticated)
            //{
            //    var referrer = Request.UrlReferrer;
            //    var host = referrer != null ? referrer.Host : "";
            //    var path = referrer != null ? referrer.LocalPath : "";
            //    var redirect = (host == Request.Url.Host ? path.ToLower().IndexOf("account/login") >= 0 : true);

            //    if (redirect)
            //    {
            //        var roles = (SimpleRoleProvider)Roles.Provider;
            //        if (roles.IsUserInRole(User.Identity.Name, AccountController.ReviewerRoleName))
            //        {
            //            // return RedirectToAction("offers", "exercises");
            //            //return RedirectToAction("Review", "Reviews", new { id = "0013AAAA" });
            //        }
            //        else
            //        {
            //            //  return RedirectToAction("index", "exercises");
            //            //return RedirectToAction("Exercise", "Reviews", new { id = "G7EAWU5L" });
            //        }
            //    }

            //    return View();
            //}
            //else
            //{
            //    return View();
            //}
        }

        public ActionResult IeltsSpeaking()
        {
            return View("Index");
        }

        public ActionResult Ielts()
        {
            return View("Index");
        }

    }
}
