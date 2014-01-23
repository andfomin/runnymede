using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Runnymede.Website.Controllers
{
    [Authorize]
    public class ExercisesController : Runnymede.Website.Utils.CustomController
    {

        // Topics.cshtml. <input type="text" class="span8" data-ng-model="vm.ownTopic.title" maxlength="100" required placeholder="Write your topic here (maximum 100 characters.)" />
        public const int MaxExerciseTitleLength = 100;

        // GET: /exercises/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /exercises/offers
        ////        public ActionResult Offers()
        ////        {
        ////            var roles = (SimpleRoleProvider)Roles.Provider;
        ////            if (!roles.IsUserInRole(User.Identity.Name, AccountController.ReviewerRoleName))
        ////            {
        ////                WebSecurity.Logout();
        ////                return RedirectToAction("Login", "Account", new { returnUrl = "/" });
        ////            }

        ////            // If the user has a review process going on, force them to finish the review first.
        ////            string reviewExtId;

        ////            using (var connection = DapperHelper.GetOpenConnection())
        ////            {
        ////                const string sql = @"
        ////select top(1) ReviewExtId
        ////from dbo.Reviews
        ////where UserId = dbo.GetUserId(@UserName)
        ////	and FinishTime is null
        ////order by StartTime;
        ////";
        ////                reviewExtId = connection.Query<string>(sql, new { UserName = User.Identity.Name }).SingleOrDefault();
        ////            }

        ////            if (!string.IsNullOrEmpty(reviewExtId))
        ////            {
        ////                return RedirectToAction("Review", "Reviews", new { id = reviewExtId });
        ////            }

        ////            return View();
        ////        }

        //// POST: /exercises/record
        //[HttpPost]
        //public ActionResult record(string title, string lines)
        //{
        //    ViewBag.TopicTitle = title;
        //    return View();
        //}

        // GET: /exercises/record?topic=12345678
        public ActionResult Record(string topic = null)
        {
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TopicsTableName);
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, topic);
            var query = new TableQuery<TopicEntity>().Where(filter);
            var entity = table.ExecuteQuery(query).Single();

            ViewBag.TopicId = topic;

            var title = entity.RowKey ?? "";
            ViewBag.TopicTitle = title;
            ViewBag.TopicShortTitle = title.Length <= MaxExerciseTitleLength ? title : title.Substring(0, MaxExerciseTitleLength);

            var lines = entity.Lines ?? "";
            IEnumerable<string> model = lines.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            return View(model);
        }

        // Do not capitalize letters within the action name. Otherwise the route will get dashes inserted whereas letters will be still lower-case anyway.
        // Audior hardcodes the path prefix, so the routing has limited options.
        // POST: /exercises/saverecording
        [HttpPost]
        public string saverecording(double duration, string recorderId = null, string userId = null)
        {
            bool success = false;
            try
            {
                using (var stream = Request.InputStream)
                {
                    UploadHelper.SaveRecording(
                                                stream,
                                                this.GetUserId(),
                                                ExerciseType.AudioRecording,
                                                Convert.ToInt32(duration * 1000),
                                                recorderId, // recorderId coming from Audior means topicId
                                                userId // userId coming from Audior means topicTitle.
                                            );
                }

                success = true;
            }
            catch (Exception ex)
            {
                return "save=failed\n" + ex.Message;
            }
            // Audior expects only "save=..." in response.
            return "save=" + (success ? "ok" : "failed");
        }

        // GET: /exercises/topics
        public ActionResult Topics()
        {
            return View();
        }

        //
        // GET: /exercises/upload?dir=BASE64_ENCODED_STRING
        ////public ActionResult upload(string dir = "")
        ////{
        ////    byte[] encodedDataAsBytes = System.Convert.FromBase64String(dir);
        ////    string userDir = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);

        ////    ViewBag.UserDir = userDir;

        ////    return View();
        ////}

        ////[HttpPost]
        ////public ActionResult upload(HttpPostedFileBase file, string userdir = "", string topic = null)
        ////{
        ////    var success = false;
        ////    try
        ////    {
        ////        if (file != null)
        ////        {
        ////            using (var stream = file.InputStream)
        ////            {
        ////                var durationMsec = UploadHelper.GetMp3DurationMsec(stream);
        ////                if (durationMsec > 0)
        ////                {
        ////                    var title = string.IsNullOrEmpty(topic)
        ////                        ? Path.GetFileNameWithoutExtension(file.FileName)
        ////                        : topic;

        ////                    UploadHelper.SaveRecording(
        ////                                                User.Identity.Name,
        ////                                                stream,
        ////                                                durationMsec,
        ////                                                null,
        ////                                                title
        ////                                            );

        ////                    success = true;
        ////                }
        ////            }

        ////            ViewBag.FileName = file.FileName;
        ////        }
        ////    }
        ////    catch (Exception)
        ////    {
        ////    }

        ////    ViewBag.Success = success;
        ////    ViewBag.UserDir = userdir;
        ////    return View();
        ////}

    }
}