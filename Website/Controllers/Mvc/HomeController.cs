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
        public ActionResult Index()
        {
            this.EnsureExtIdCookie();
            return View();
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
