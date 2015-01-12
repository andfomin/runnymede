using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class InfoController : Runnymede.Website.Utils.CustomController
    {
        public ActionResult Faq()
        {
            return View();
        }

        public ActionResult TermsOfService()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Dignicom()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult SupportedBrowsers()
        {
            return View();
        }

        public ActionResult UnsupportedBrowser()
        {
            return View();
        }

    }
}