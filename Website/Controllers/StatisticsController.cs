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
select distinct E.CreateTime, E.Title
from dbo.exeExercises E
	inner join dbo.exeReviews R on E.Id = R.ExerciseId
where E.UserId = @UserId
	and R.FinishTime is not null
order by E.CreateTime;
";
            var exercises = await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { UserId = this.GetUserId() });
            ViewBag.ExercisesParamJson = ControllerHelper.SerializeAsJson(exercises);
            return View();
        }
	}
}