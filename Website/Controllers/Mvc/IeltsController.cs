using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class IeltsController : Controller
    {
        //
        // GET: /ielts/practice-speaking/abc
        public ActionResult PracticeSpeaking(string id)
        {
            return View();
        }
	}
}