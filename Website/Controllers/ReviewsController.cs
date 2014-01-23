using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Web.Configuration;
using Dapper;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System.Xml.Linq;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
//using System.Web.Http;

namespace Runnymede.Website.Controllers
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
            if (!this.GetIsTutor())
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
        public async Task<ActionResult> Edit(int id = 0) // by Review Id
        {
            var dto = await GetDtoForEdit(id);
            return IntenalGet(dto, "Edit");
        }

        // GET: /reviews/view/12345 
        [ActionName("View")]
        public async Task<ActionResult> ViewAll(int id) // by Exercise Id
        {
            var dto = await GetDtoForView(id);
            return IntenalGet(dto, "View");
        }

        private async Task<ExerciseDto> GetDtoForEdit(int reviewid)
        {
            const string sql = @"
select E.Id, E.[Length], E.TypeId, E.ArtefactId, R.ExerciseId, R.Id, R.StartTime, R.FinishTime
from dbo.exeExercises E 	
	inner join dbo.exeReviews as R on E.Id = R.ExerciseId
where R.Id = @Id and R.UserId = @UserId;
";

            using (var connection = DapperHelper.GetOpenConnection())
            {
                return (await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                                sql,
                                (e, r) => { e.Reviews = new[] { r }; return e; },
                                new { Id = reviewid, UserId = this.GetUserId() },
                                splitOn: "ExerciseId"
                            ))
                        .SingleOrDefault();
            }
        }

        private async Task<ExerciseDto> GetDtoForView(int exerciseid)
        {
            const string sql = @"
select E.Id, E.[Length], E.TypeId, E.ArtefactId, E.CreateTime, E.Title, R.ExerciseId, R.Id, R.StartTime, R.FinishTime
from dbo.exeExercises E 	
	left join dbo.exeReviews as R on E.Id = R.ExerciseId
where E.Id = @Id and E.UserId = @UserId;
";

            IEnumerable<ExerciseDto> dtos;
            using (var connection = DapperHelper.GetOpenConnection())
            {
                dtos = (await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                                sql,
                                (e, r) => { e.Reviews = new[] { r }; return e; },
                                new { Id = exerciseid, UserId = this.GetUserId() },
                                splitOn: "ExerciseId"
                            ));
            }

            // We got a set of the same Exercise, each with a single Review. Group them into a single Exercise.               
            var reviews = dtos
                .SelectMany(i => i.Reviews)
                .Where(r => r is ReviewDto) // Absent review is deserialized by Dapper as null. i.e [null].
                .ToList(); // ToList() is needed. Otherwise a mistical StackOverflow occurs within IIS.

            // Use the first Exercise as a single instance. Make it the common parent of all Reviews.
            dtos.First().Reviews = reviews;

            return dtos.First();
        }

        private ActionResult IntenalGet(ExerciseDto dto, string viewName)
        {
            if (dto == null)
            {
                return HttpNotFound();
            }

            ViewBag.SoundUrlParam = AzureStorageUtils.GetRecordingsBaseUrl() + dto.ArtefactId;
            ViewBag.ExerciseParamJson = ControllerHelper.SerializeAsJson(dto);

            return View(viewName);
        }


        /* Use HTTPS to avoid HTTP referer header on redirect. +http://en.wikipedia.org/wiki/HTTP_referer#cite_note-10 */
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