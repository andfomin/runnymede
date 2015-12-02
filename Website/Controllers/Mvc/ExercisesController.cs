using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class ExercisesController : Runnymede.Website.Utils.CustomController
    {
        // GET: /exercises/
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /exercises/record-speech
        [AllowAnonymous]
        public ActionResult RecordSpeech()
        {
            return View();
        }

        // GET: /exercises/photograph-writing
        [AllowAnonymous]
        public ActionResult PhotographWriting()
        {
            return View();
        }

        // GET: /view/2000000001 by ExerciseId
        [ActionName("View")] // Be carefull, the term "View" has a special meaning in ASP.NET MVC. There is the method View() in the base Controller class.
        public async Task<ActionResult> ViewExercise(int id)
        {
            var exercises = await ExerciseUtils.GetExerciseWithReviews("dbo.exeGetExerciseWithReviews", new { UserId = this.GetUserId(), Id = id, });

            // We got a few rows of the same Exercise, joined with different Reviews. Group them into a single Exercise.               
            var reviews = exercises
                .SelectMany(i => i.Reviews)
                .Where(i => i is ReviewDto) // Otherwise an absent review is deserialized by Dapper as null. i.e [null].
                .OrderBy(i => i.RequestTime)
                .ToList(); // ToList() is needed, otherwise a mistical StackOverflow occurs within IIS.
            // Use the first Exercise as a single instance. Make it the common parent of all Reviews.
            var exercise = exercises.First();
            exercise.Reviews = reviews;

            ViewBag.ExerciseParam = exercise;
            ViewBag.cardIdParam = exercise.CardId.GetValueOrDefault();
            if (exercise.CardId.HasValue)
            {
                ViewBag.cardParam = await ExerciseUtils.GetCardWithItems(exercise.CardId.Value);
            }

            switch (exercise.ArtifactType)
            {
                case ArtifactType.Jpeg:
                    return View("ViewPhoto");
                case ArtifactType.Mp3:
                    return View("ViewRecording");
                default:
                    return HttpNotFound(exercise.ArtifactType);
            }
        }

        // GET: /exercises/upload?dir=BASE64_ENCODED_STRING
        public ActionResult Upload(string dir = "")
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(dir);
            string userDir = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            ViewBag.UserDir = userDir;
            return View();
        }

        // The action name corresponds to the path in audior_settings.xml.
        // Audior hardcodes the path prefix, so routing has limited options.
        // POST: /exercises/save-audior-recording
        [HttpPost]
        public async Task<string> SaveAudiorRecording(string recorderId, string recordName, double duration)
        {
            bool success = false;
            try
            {
                var userId = this.GetUserId();
                // recorderId comes from Audior, 13 digits, milliseconds, current Date in the browser. It is part of Artifact and will be used to update Title with the real title after upload is done.
                var blobName = UploadUtils.ConstractArtifactBlobName(userId, recorderId, recordName);

                using (var stream = Request.InputStream)
                {
                    await AzureStorageUtils.UploadBlobAsync(stream, AzureStorageUtils.ContainerNames.Artifacts, blobName, MediaType.Mp3);
                }

                var length = UploadUtils.DurationToLength(duration);
                await ExerciseUtils.CreateExercise(blobName, userId, ServiceType.IeltsSpeaking, ArtifactType.Mp3, length, null);
                success = true;
            }
            catch (Exception ex)
            {
                return "save=failed\n" + ex.Message;
            }
            // Audior expects only "save=..." in response.
            return "save=" + (success ? "ok" : "failed");
        }

        // POST: /exercises/save-recording-mobile
        [HttpPost]
        public async Task<ActionResult> SaveRecordingMobile(HttpPostedFileBase fileInput, string recorderId, string recordingTitle)
        {
            /*
             * 1. Save the original media file to a blob
             * 2. Call the remote Transcoding service. Pass the blob name.
             * 3. The Transcoding service saves the MP3 file to a blob and returns its name and the recording's duration.
             * 4. Create a database record.             
             */
            if (fileInput == null)
            {
                return RedirectToAction("RecordSpeech", new { error = "no_file" });
            }

            //  return RedirectToAction("RecordSpeech", new { error = "test01" });

            var contentType = fileInput.ContentType;
            var acceptedContentType = (new[] { MediaType.Amr, MediaType.Gpp, MediaType.QuickTime }).Contains(contentType);
            if (!acceptedContentType)
            {
                return RedirectToAction("RecordSpeech", new { error = contentType });
            }

            var userId = this.GetUserId();

            // 1. Save the original file.
            // The directory structure in Blob Storage is userIdKey/timeKey/originalFileName.ext
            var timeKey = KeyUtils.GetTimeAsBase32();
            var fileName = fileInput.FileName;
            // Sanitize the fileName. Reserved URL characters must be properly escaped.
            fileName = String.IsNullOrWhiteSpace(fileName)
                ? timeKey
                : Uri.EscapeUriString(fileName.Trim());
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = new String(fileName.Select(i => invalidChars.Contains(i) ? '_' : i).ToArray());
            if (!Path.HasExtension(fileName))
            {
                Path.ChangeExtension(fileName, MediaType.GetExtension(contentType));
            }

            var blobName = KeyUtils.IntToKey(userId) + AzureStorageUtils.DefaultDirectoryDelimiter + timeKey + AzureStorageUtils.DefaultDirectoryDelimiter + fileName;
            using (var stream = fileInput.InputStream)
            {
                await AzureStorageUtils.UploadBlobAsync(stream, AzureStorageUtils.ContainerNames.Recordings, blobName, contentType);
            }

            // 2. Call the transcoding service.
            var host = ConfigurationManager.AppSettings["RecorderHost"];
            var url = String.Format("http://{0}/api/recordings/transcoded/?inputBlobName={1}", host, blobName);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("RecordSpeech", new { error = "transcoding_error" });
            }
            // 3. Get the results from the service.
            // Error is returned as HTML. Then we get error here: No MediaTypeFormatter is available to read an object of type 'JObject' from content with media type 'text/html'.
            var value = await response.Content.ReadAsAsync<JObject>();
            var outputBlobName = (string)value["outputBlobName"];
            var durationMsec = (int)value["durationMsec"];
            // The transcoder may return -1 if it has failed to parse the ffmpeg logs.
            if (durationMsec < 0)
            {
                // Read the blob and try to determine the duration directly.
                var outputBlob = AzureStorageUtils.GetBlob(AzureStorageUtils.ContainerNames.Recordings, outputBlobName);
                using (var stream = new MemoryStream())
                {
                    await outputBlob.DownloadToStreamAsync(stream);
                    durationMsec = RecordingUtils.GetMp3DurationMsec(stream); // Seeks the stream to the beginning internally.
                }
            }
            // 4. Create a database record.
            var exerciseId = await ExerciseUtils.CreateExercise(outputBlobName, userId, ServiceType.IeltsSpeaking, ArtifactType.Mp3, durationMsec, recordingTitle);
            // 5. Redirect to the exercise page.
            return RedirectToAction("View", new { Id = exerciseId });
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
                        var durationMsec = RecordingUtils.GetMp3DurationMsec(stream);
                        if (durationMsec > 0)
                        {
                            var recordingTitle = !String.IsNullOrEmpty(title)
                                ? title
                                : Path.GetFileNameWithoutExtension(mp3File.FileName);

                            exerciseId = await ExerciseUtils.SaveRecording(
                                stream,
                                userId,
                                ArtifactType.Mp3,
                                ServiceType.IeltsSpeaking,
                                durationMsec,
                                recordingTitle
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
                if (!String.IsNullOrEmpty(learnerSkype))
                {
                    newUserId = (await DapperHelper.QueryResilientlyAsync<int?>("dbo.exeTryChangeExerciseAuthor",
                                             new
                                             {
                                                 ExerciseId = exerciseId,
                                                 UserId = userId,
                                                 SkypeName = learnerSkype,
                                             },
                                             CommandType.StoredProcedure))
                                             .SingleOrDefault()
                                             .GetValueOrDefault();
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
                        var pieces = remarkSpots
                            .OrderBy(i => i)
                            .Distinct()
                            .Select((i, index) =>
                            {
                                var finish = Math.Max(0, i - 1000); // We allow for a 1 second delay between the actual learner's mistake and the teacher's action.
                                var start = Math.Max(0, finish - 2000); // Assume the spot is 2 second long.

                                var remark = new RemarkSpot
                                {
                                    ReviewId = reviewId,
                                    Type = ReviewPiece.PieceTypes.Remark,
                                    Id = index,
                                    Start = start,
                                    Finish = finish,
                                };

                                return new ReviewPiece
                                {
                                    PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                                    RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Remark, index),
                                    Json = JsonUtils.SerializeAsJson(remark),
                                };
                            });

                        var batchOperation = new TableBatchOperation();
                        foreach (var piece in pieces)
                        {
                            batchOperation.InsertOrReplace(piece);
                        }

                        // Write entities which will allow the reviewer to access remarks for reading and writing. We will simply check the presence of one of these records as we read or write the entities.
                        // The write entity will be deleted on review finish.
                        var viewerEntity = new ReviewPiece()
                        {
                            PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                            RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Viewer, userId),
                        };
                        batchOperation.InsertOrReplace(viewerEntity);

                        var editorEntity = new ReviewPiece()
                        {
                            PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                            RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Editor, userId),
                        };
                        batchOperation.InsertOrReplace(editorEntity);

                        var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
                        await table.ExecuteBatchAsync(batchOperation);

                        // Redirect to the reviews page.
                        return RedirectToAction("Edit", "Reviews", new { id = reviewId.ToString() });
                    }
                } // end of if (remarkSpots.Any())
            } // end of if (exerciseId && this.GetIsTeacher())

            //
            if (exerciseId != 0)
            {
                return newUserId == 0 ? RedirectToAction("Index") : RedirectToAction("Index", "Reviews");
            }

            ViewBag.Success = exerciseId != 0;
            ViewBag.UserDir = userdir;

            return View();
        } // end of Upload()

        // This method is called as by redirection after signup which in turn was called by the Speaking page after the recording had been saved by an unauthorized user. The id (aka timeKey) is passed along as the returnUrl parameter in URLs.
        // GET: /exercises/claim/adcd1234
        public async Task<ActionResult> Claim(string id)
        {
            var userIdKey = KeyUtils.IntToKey(0);
            var longTimeKey = id + this.GetExtId();
            var blobName = ExerciseUtils.FormatBlobName(userIdKey, longTimeKey, "metadata", "json");
            var blob = AzureStorageUtils.GetBlob(AzureStorageUtils.ContainerNames.Artifacts, blobName);
            var metadataJson = await blob.DownloadTextAsync();
            var metadata = JObject.Parse(metadataJson);
            var serviceType = (string)metadata["serviceType"];
            var cardId = (Guid?)metadata["cardId"];
            var title = (string)metadata["title"];
            var comment = (string)metadata["comment"];
            var details = metadata["recordingDetails"];
            var recordingDetails = details.ToObject<RecordingDetails>();

            var exerciseId = await ExerciseUtils.CreateExercise(recordingDetails.BlobName, this.GetUserId(),
                serviceType, ArtifactType.Mp3, recordingDetails.TotalDuration, title, cardId, comment, details.ToString(Formatting.None));

            //~~ Redirect to the View exercise page.
            return RedirectToAction("View", new { Id = exerciseId });
        }

    }
}