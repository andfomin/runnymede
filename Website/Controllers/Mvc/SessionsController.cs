using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Runnymede.Common.Utils;
using System.Web.Hosting;
using Runnymede.Website.Utils;
using Runnymede.Common.Models;
using System.Threading.Tasks;
using System.Net;
using Twilio;
using System.Configuration;

namespace Runnymede.Website.Controllers.Mvc
{
    public class SessionsController : Runnymede.Website.Utils.CustomController
    {

        // GET: sessions/
        public ActionResult Index()
        {
            HostingEnvironment.QueueBackgroundWorkItem(ct => ItalkiHelper.EnsureIsReady());
            return View();
        }

        // GET: sessions/teacher
        public ActionResult Teacher()
        {
            if (!this.GetUserIsTeacher())
            {
                return Redirect(Url.Action("Logout", "Account", null, "https"));
            }
            HostingEnvironment.QueueBackgroundWorkItem(ct => ItalkiHelper.EnsureIsReady());
            return View();
        }

        //        public ActionResult Test01()
        //        {
        //            /* The View contents:
        //<form action="/api/sessions/inbound_webhook" method="post">
        //    <input type="text" name="mandrill_events" value="qwe1" />
        //    <input type="submit" value="Ok" />
        //</form>            
        //            */
        //            return View();
        //        }

        // GET: sessions/skype-recorder

        [Authorize]
        public ActionResult SkypeRecorder()
        {
            if (!this.GetUserIsTeacher())
            {
                return Redirect(Url.Action("Logout", "Account", null, "https"));
            }
            return View();
        }



    }
}