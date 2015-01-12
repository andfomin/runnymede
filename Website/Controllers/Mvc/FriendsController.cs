using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class FriendsController : Runnymede.Website.Utils.CustomController
    {
        // GET: /partners
        public ActionResult Index()
        {
            return View();
        }
    }
}