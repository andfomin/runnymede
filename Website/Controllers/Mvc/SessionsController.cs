using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Runnymede.Website.Utils;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class SessionsController : Controller
    {
        // GET: /sessions/teacher
        public ActionResult Teacher()
        {
            if (!this.GetUserIsTeacher())
            {
                return RedirectToAction("Logout", "Account");
            }

            return View();
        }

        // GET: /sessions/learner
        public ActionResult Learner()
        {
            if (this.GetUserIsTeacher())
            {
                return RedirectToAction("Logout", "Account");
            }

            return View();
        }

	}
}