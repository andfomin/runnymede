using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class SkypeRecorderController : Controller
    {
        // GET: /skype-recorder/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /skype-recorder/error
        public ActionResult Error()
        {
            return View("Index");
        }
    }
}