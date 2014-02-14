using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Runnymede.Website.Utils;
using System.Threading.Tasks;
using System.Data;

namespace Runnymede.Website.Controllers
{
    [Authorize]
    public class RelationshipsController : Runnymede.Website.Utils.CustomController
    {

        // GET: /relationships/tutors
        public ActionResult Tutors()
        {
            return View();
        }

        // GET: /relationships/tutor-learners
        public ActionResult TutorLearners()
        {
            return View("TutorLearners");
        }

        // GET: /relationships/skype-directory
        // GET: /relationships/skype-directory/join
        public async Task<ActionResult> SkypeDirectory(string id)
        {
            var join = !string.IsNullOrEmpty(id) && (id.ToLower() == "join");

            var sqlJoined = @"
select count(*) from dbo.relSkypeDirectory
where UserId = @UserId
	and sysutcdatetime() between TimeBegin and coalesce(TimeEnd, '2100-01-01');
";
            var joined = (await DapperHelper.QueryResilientlyAsync<int>(sqlJoined, new { UserId = this.GetUserId() })).First() > 0;

            if (join && joined)
            {
                return RedirectToAction("SkypeDirectory");
            }

            if (!join && !joined)
            {
                return RedirectToAction("SkypeDirectory", new { id = "join" });
            }

            if (!joined)
            {
                var sqlProfile = @"
select Skype
from dbo.appUsers 
where Id = @UserId;
";
                var profile = (await DapperHelper.QueryResilientlyAsync<dynamic>(sqlProfile, new { UserId = this.GetUserId() })).First();
                ViewBag.Skype = (string)profile.Skype;
            }

            return View(join ? "SkypeDirectoryJoin" : "SkypeDirectory");
        }

    }
}