﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Runnymede.Website.Utils;

namespace Runnymede.Website.Controllers.Mvc
{
    public class SessionsController : Runnymede.Website.Utils.CustomController
    {

        // GET: sessions/
        public ActionResult Index()
        {
            return View();
        }

	}
}