using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class GamesController : Runnymede.Website.Utils.CustomController
    {
        // GET: games/pick-a-pic
        public ActionResult PickAPic()
        {
            return View();
        }

        // GET: games/copycat
        public ActionResult Copycat()
        {
            return View();
        }

        // GET: games/copycat-add
        public ActionResult CopycatAdd()
        {
            return View();
        }

    }
}