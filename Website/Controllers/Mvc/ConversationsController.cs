using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class ConversationsController : Controller
    {
        // GET: conversations/
        public ActionResult Index()
        {
            return View();
        }
    }
}