using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class ResourcesController : Controller
    {
        // GET: resources?q=qwerty
        public ActionResult Index(string q)
        {
            string rawQuery = HttpUtility.UrlDecode(q);

            // Sanitize the query text.
            var keywords = (rawQuery ?? "")
                .Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.ToLower())
                .ToList();

            var query = string.Join(" ", keywords);





            return View();
        }
        // GET: resources/youtube
        // Do not capitalize in the middle, otherwise the route will get a dash inserted. (YouTube => you-tube)
        public ActionResult Youtube()
        {
            return View();
        }

    }
}