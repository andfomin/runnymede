using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class TeachersController : Runnymede.Website.Utils.CustomController
    {
        //
        // GET: /Teachers/
        public ActionResult Index()
        {
            return View();
        }
	}
}