using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers
{
    [Authorize]
    public class StatisticsController : Runnymede.Website.Utils.CustomController
    {
        //
        // GET: /Statistics/
        public async Task<ActionResult> Index()
        {
                const string sql = @"
select CreateTime, Title
from dbo.exeExercises
where UserId = @UserId
order by CreateTime;
";
            var exercises = await DapperHelper.QueryResilientlyAsync<ExerciseDto>(sql, new { UserId = this.GetUserId() });
            ViewBag.ExercisesParamJson = ControllerHelper.SerializeAsJson(exercises);
            return View();
        }
	}
}