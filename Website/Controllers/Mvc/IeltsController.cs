using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class IeltsController : Controller
    {
        public ActionResult Index()
        {
            //return RedirectToAction("Index", "Home"); // returns 302 Moved
            return View("../Home/Index");
        }

        public ActionResult Info()
        {
            return View("../Home/Index");
        }

        public ActionResult Speaking()
        {
            return View("../Home/Index");
        }

        public ActionResult Writing()
        {
            return View("../Home/Index");
        }

    }
}