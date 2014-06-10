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
    [Authorize]
    public class ReviewsController : Runnymede.Website.Utils.CustomController
    {

        // GET: /reviews/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /reviews/requests
        public async Task<ActionResult> Requests()
        {
            if (!this.GetUserIsTeacher())
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/" });
            }

            const string sql = @"
select Id from dbo.exeReviews where UserId = @UserId and FinishTime is null;
";
            var reviewId = (await DapperHelper.QueryResilientlyAsync<int>(sql, new { UserId = this.GetUserId() })).FirstOrDefault();
            ViewBag.ReviewId = reviewId;

            return View();
        }

        // GET: /reviews/edit/12345
        public async Task<ActionResult> Edit(int id)
        {
            var exercise = await GetExerciseForEdit(id); // for ReviewId
            return IntenalGet(exercise, "Edit");
        }

        // GET: /reviews/view/12345 
        [ActionName("View")]
        public async Task<ActionResult> ViewAll(int id)
        {
            var exercise = await GetExerciseForViewAll(id); // for ExerciseId
            return IntenalGet(exercise, "View");
        }

        private async Task<ExerciseDto> GetExerciseForEdit(int reviewid)
        {
            const string sqlExercise = @"
select E.Id, E.[Length], E.TypeId, E.ArtefactId, R.ExerciseId, R.Id, R.StartTime, R.FinishTime, R.Comment
from dbo.exeExercises E 	
	inner join dbo.exeReviews as R on E.Id = R.ExerciseId
where R.Id = @Id 
    and R.UserId = @UserId;
";

            const string sqlSuggestions = @"
select S.ReviewId, S.CreationTime, S.[Text]
from dbo.exeSuggestions S
	inner join dbo.exeReviews R on S.ReviewId = R.Id
where R.Id = @ReviewId
	and R.UserId = @UserId ;
";

            ExerciseDto exercise;
            IEnumerable<SuggestionDto> suggestions;
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                exercise = (await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                                sqlExercise,
                                (e, r) => { e.Reviews = new[] { r }; return e; },
                                new { Id = reviewid, UserId = this.GetUserId() },
                                splitOn: "ExerciseId"
                            ))
                        .Single();

                suggestions = (await connection.QueryAsync<SuggestionDto>(sqlSuggestions, new { ReviewId = reviewid, UserId = this.GetUserId() })).ToList();
            }

            exercise.Reviews.Single().Suggestions = suggestions;
            return exercise;
        }

        private async Task<ExerciseDto> GetExerciseForViewAll(int exerciseId)
        {
            const string sqlExercise = @"
select E.Id, E.[Length], E.TypeId, E.ArtefactId, E.CreateTime, E.Title, R.ExerciseId, R.Id, R.StartTime, R.FinishTime, R.ReviewerName, R.Comment
from dbo.exeExercises E 	
	left join dbo.exeReviews as R on E.Id = R.ExerciseId
where E.Id = @Id and E.UserId = @UserId;
";
            const string sqlSuggestions = @"
select S.ReviewId, S.CreationTime, S.[Text]
from dbo.exeSuggestions S
	inner join dbo.exeReviews RV on S.ReviewId = RV.Id
	inner join dbo.exeExercises E on RV.ExerciseId = E.Id
where E.Id = @ExerciseId
	and E.UserId = @UserId;
";
            IEnumerable<ExerciseDto> exercises;
            IEnumerable<SuggestionDto> suggestions;
            using (var connection = DapperHelper.GetOpenConnection())
            {
                exercises = (await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                                sqlExercise,
                                (e, r) => { e.Reviews = new[] { r }; return e; },
                                new { Id = exerciseId, UserId = this.GetUserId() },
                                splitOn: "ExerciseId"
                            ));

                suggestions = (await connection.QueryAsync<SuggestionDto>(sqlSuggestions, new { ExerciseId = exerciseId, UserId = this.GetUserId() })).ToList();
            }

            // Use the first Exercise as a single instance. Make it the common parent of all Reviews.
            var exercise = exercises.First();

            // We got a few rows of the same Exercise, each with a single Review. Group them into a single Exercise.               
            var reviews = exercises
                .SelectMany(i => i.Reviews)
                .Where(i => i is ReviewDto) // Otherwise an absent review is deserialized by Dapper as null. i.e [null].
                .OrderBy(i => i.RequestTime)
                .ToList(); // ToList() is needed. Otherwise a mistical StackOverflow occurs within IIS.

            exercise.Reviews = reviews;

            foreach (var review in reviews)
            {
                review.Suggestions = suggestions.Where(i => i.ReviewId == review.Id).ToList();
            }

            return exercise;
        }

        private ActionResult IntenalGet(ExerciseDto exercise, string viewName)
        {
            if (exercise == null)
            {
                return HttpNotFound();
            }

            ViewBag.ExerciseParamJson = LoggingUtils.SerializeAsJson(exercise);
            ViewBag.SoundUrlParam = AzureStorageUtils.GetContainerBaseUrl(AzureStorageUtils.ContainerNames.Recordings) + exercise.ArtefactId;

            return View(viewName);
        }

        /* Use HTTPS to avoid HTTP referer header on redirect to BCEG. +http://en.wikipedia.org/wiki/HTTP_referer#cite_note-10 */
        //[RequireHttps]
        public async Task<ActionResult> TagSearch(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return HttpNotFound(q);
            }

            string csv = HttpUtility.UrlDecode(q);

            // Sanitize the CSV text.
            var tags = (string.IsNullOrWhiteSpace(csv) ? "" : csv)
                    .Split(',')
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();

            IEnumerable<string> values = new List<string>();

            if (tags.Any())
            {
                var singleTag = tags.Count() == 1;

                string sql2 = @"
select [Value] from dbo.exeTagSearch where " + (singleTag ? "Tag = @Tags;" : "Tag in @Tags;");

                if (singleTag)
                {
                    values = await DapperHelper.QueryResilientlyAsync<string>(sql2, new { Tags = tags.First() });
                }
                else
                {
                    values = await DapperHelper.QueryResilientlyAsync<string>(sql2, new { Tags = tags });
                }

                // If at least one tag is a valid BCEG one, go to BCEG and ignore the free-form ones. Otherwise go to Google.
                switch (values.Count())
                {
                    case 0:
                        return Redirect("https://www.google.com/search?q=" + HttpUtility.UrlEncode(string.Join("&", tags)));
                    case 1:
                        return Redirect(values.First());
                    default:
                        return View(values);
                }
            }

            return HttpNotFound(q);
        }

        //+http://localhost:2453/reviews/writestats?reviewId=11&userId=4
        public async Task<int> WriteStats(int reviewId, int userId)
        {
            return await StatisticsUtils.WriteTagStatistics(reviewId, userId);
        }


    }
}