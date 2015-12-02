using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class StorytellingController : Controller
    {
        // GET: Storytelling
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Speak()
        {
            return View();
        }

        //public ActionResult Listen()
        //{
        //    return View();
        //}


    }
}