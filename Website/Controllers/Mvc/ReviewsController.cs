using Dapper;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class ReviewsController : Runnymede.Website.Utils.CustomController
    {

        // GET: /reviews/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /reviews/edit/12345 by ReviewId
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            const string sql = @"
select E.Id, E.[Length], E.[Type], E.Artifact, 
    R.ExerciseId, R.Id, R.StartTime, R.FinishTime
from dbo.exeExercises E 	
	inner join dbo.exeReviews as R on E.Id = R.ExerciseId
where R.Id = @Id 
    and R.UserId = @UserId;
";
            var exercise = (await QueryExerciseReviews(sql, id, this.GetUserId())).Single();

            ViewBag.ExerciseParam = exercise;

            switch (exercise.Type)
            {
                case ExerciseType.WritingPhoto:
                    return View("EditWriting");
                case ExerciseType.AudioRecording:
                    return View("EditRecording");
                default:
                    return HttpNotFound(exercise.Type);
            }
        }

        internal static async Task<IEnumerable<ExerciseDto>> QueryExerciseReviews(string sql, int entityId, int userId)
        {
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                return await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                    sql,
                    (e, r) => { e.Reviews = new[] { r }; return e; },
                    new { Id = entityId, UserId = userId },
                    splitOn: "ExerciseId");
            }
        }

    }
}