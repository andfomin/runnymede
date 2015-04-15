using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class RecorderForSkypeController : Controller
    {
        // GET: recorder-for-skype/
        public ActionResult Index()
        {
            return View();
        }

        // GET: recorder-for-skype/help/?topic=X
        public ActionResult Help(int topic = 0)
        {
            string url = null;
            // 0 - the ? button on the form pressed
            // 1 - The recorder was unable to attach to Skype.
            // 2 - The recorder can be started only when a call is in progress.
            switch (topic)
            {
                default:
                    url = "http://support.englisharium.com/support/solutions/articles/5000546743-recorder-for-skype";
                    break;
            }

            return Redirect(url);
        }

        // GET: recorder-for-skype/upload/?dir=BASE64_ENCODED_LOCAL_DIR
        public ActionResult Upload(string dir = null)
        {
            return RedirectToAction("Upload", "Exercises", new { dir = dir });
        }

    }
}