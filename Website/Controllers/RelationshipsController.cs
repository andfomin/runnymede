using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers
{
    [Authorize]
    public class RelationshipsController : Runnymede.Website.Utils.CustomController
    {
        //
        // GET: /Relationships/
        public ActionResult Index()
        {
            return View();
        }
    }
}