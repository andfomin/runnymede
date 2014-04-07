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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class ExercisesController : Runnymede.Website.Utils.CustomController
    {
        // GET: /exercises/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /exercises/record?topic=12345678
        public ActionResult Record(string topic = null)
        {
            IEnumerable<string> model = null;
            if (!string.IsNullOrEmpty(topic))
            {
                var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TopicsTableName);
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, topic);
                var query = new TableQuery<TopicEntity>().Where(filter);
                var entity = table.ExecuteQuery(query).Single();

                ViewBag.TopicId = topic;

                var title = entity.RowKey ?? "";
                ViewBag.TopicTitle = title;
                ViewBag.TopicShortTitle = title.Length <= UploadHelper.MaxExerciseTitleLength ? title : title.Substring(0, UploadHelper.MaxExerciseTitleLength);

                var lines = entity.Lines ?? "";
                model = lines.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
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

        // GET: /exercises/upload?dir=BASE64_ENCODED_STRING
        public ActionResult Upload(string dir = "")
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(dir);
            string userDir = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);

            ViewBag.UserDir = userDir;

            return View();
        }

        // POST: /exercises/upload
        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase mp3File, HttpPostedFileBase xmlFile, string userdir = "", string title = null, string learnerSkype = null)
        {
            int exerciseId = 0;
            int newUserId = 0;

            var userId = this.GetUserId();

            // Save the recording
            if (mp3File != null)
            {
                ViewBag.mp3FileName = mp3File.FileName;
                // We can receive garbage from the wild web.
                try
                {
                    using (var stream = mp3File.InputStream)
                    {
                        var durationMsec = UploadHelper.GetMp3DurationMsec(stream);
                        if (durationMsec > 0)
                        {
                            var exerciseTitle = !string.IsNullOrEmpty(title)
                                ? title
                                : Path.GetFileNameWithoutExtension(mp3File.FileName);

                            exerciseId = UploadHelper.SaveRecording(
                                stream,
                                userId,
                                ExerciseType.AudioRecording,
                                durationMsec,
                                null,
                                exerciseTitle
                                );
                        }
                    }
                }
                catch
                {
                }
            }

            // If the recording is being uploaded by a teacher, make it owned by the learner, create a review, and save remark spots if an accompanying XML file is provided.
            if (exerciseId != 0 && this.GetUserIsTeacher())
            {
                // We may succeed or fail with updating the user of the exercise depending on whether the provided Skype name is found and is unambiguous.
                // Continue anyway with either old or new user.
                if (!string.IsNullOrEmpty(learnerSkype))
                {
                    newUserId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.exeTryChangeExerciseAuthor",
                                             new
                                             {
                                                 ExerciseId = exerciseId,
                                                 UserId = userId,
                                                 Skype = learnerSkype,
                                             },
                                             CommandType.StoredProcedure))
                                             .FirstOrDefault();
                }

                IEnumerable<int> remarkSpots = new int[] { };

                // If there is a remark spot collection XML file produced by the teacher coming with the recording.
                if (xmlFile != null)
                {
                    // We can receive garbage from the wild web.
                    try
                    {
                        XElement root;
                        using (var stream = xmlFile.InputStream)
                        {
                            root = XElement.Load(stream);
                        }

                        var startedText = root.Attribute("time").Value;
                        var started = DateTime.Parse(startedText, null, DateTimeStyles.RoundtripKind);

                        remarkSpots = root
                            .Elements("RemarkSpot")
                            .Select(i =>
                            {
                                var spot = DateTime.Parse(i.Attribute("time").Value, null, DateTimeStyles.RoundtripKind);
                                return Convert.ToInt32((spot - started).TotalMilliseconds);
                            })
                            .ToList();
                    }
                    catch
                    {
                    }
                }

                // We have got remark spots. Create a review.
                if (remarkSpots.Any())
                {
                    var reviewId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.exeCreateUploadedReview", new
                    {
                        ExerciseId = exerciseId,
                        UserId = userId,
                    },
                    CommandType.StoredProcedure
                    ))
                    .SingleOrDefault();

                    // Save remarks.
                    if (reviewId != 0)
                    {
                        var partitionKey = AzureStorageUtils.IntToKey(reviewId);

                        var entities = remarkSpots
                            .OrderBy(i => i)
                            .Distinct()
                            .Select(i =>
                            {
                                var finish = Math.Max(0, i - 1000); // We allow for a 1 second delay between the actual learner's mistake and the teacher's action.
                                var start = Math.Max(0, finish - 2000); // Assume the spot is 2 second long.
                                // Make Id fixed-length sequecial. The clustered index in Azure Table is PartitionKey+RowKey.
                                var remarkId = ConvertToSixDigitBase36((start + finish) / 2);

                                return new RemarkEntity
                                {
                                    PartitionKey = partitionKey,
                                    RowKey = remarkId,
                                    Start = start,
                                    Finish = finish,
                                };
                            });

                        var batchOperation = new TableBatchOperation();
                        entities.ToList().ForEach(i => batchOperation.InsertOrReplace(i));
                        var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.RemarksTableName);
                        await table.ExecuteBatchAsync(batchOperation);

                        // Redirect to the reviews page.
                        return RedirectToAction("Edit", "Reviews", new { id = reviewId.ToString() });
                    }
                }
            } // end of if (mp3Success && this.GetIsTeacher())

            //
            if (exerciseId != 0)
            {
                return newUserId == 0 ? RedirectToAction("Index") : RedirectToAction("Index", "Reviews");
            }

            ViewBag.Success = exerciseId != 0;
            ViewBag.UserDir = userdir;

            return View();
        }

        private string ConvertToSixDigitBase36(int number)
        {
            const int radix = 36;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Base36
            char[] charArray = { '0', '0', '0', '0', '0', '0' }; // The number is padded with zeros at the beginning.
            int index = 5;

            while (number != 0 && index > 0)
            {
                int remainder = (int)(number % radix);
                charArray[index--] = Digits[remainder];
                number = number / radix;
            }

            return new String(charArray);
        }


    }
}