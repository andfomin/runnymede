using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using Dapper;
using Runnymede.Common.Utils;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;

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
        [ActionName("View")] // Be carefull, the term "View" has a special meaning in ASP.NET MVC. There is method View() in the base class.
        public async Task<ActionResult> ViewExercise(int id)
        {
            const string sql = @"
select E.Id, E.[Length], E.[Type], E.Artifact, E.CreateTime, E.Title, 
    R.ExerciseId, R.Id, R.RequestTime, R.StartTime, R.FinishTime, R.UserId, R.ReviewerName
from dbo.exeExercises E 	
	left join dbo.exeReviews as R on E.Id = R.ExerciseId and R.CancelationTime is null
where E.Id = @Id 
    and E.UserId = @UserId;
";
            var exercises = await ReviewsController.QueryExerciseReviews(sql, id, this.GetUserId());

            // We got a few rows of the same Exercise, each with a single Review. Group them into a single Exercise.               
            var reviews = exercises
                .SelectMany(i => i.Reviews)
                .Where(i => i is ReviewDto) // Otherwise an absent review is deserialized by Dapper as null. i.e [null].
                .OrderBy(i => i.RequestTime)
                .ToList(); // ToList() is needed, otherwise a mistical StackOverflow occurs within IIS.
            // Use the first Exercise as a single instance. Make it the common parent of all Reviews.
            var exercise = exercises.First();
            exercise.Reviews = reviews;

            ViewBag.ExerciseParam = exercise;

            switch (exercise.Type)
            {
                case ExerciseType.WritingPhoto:
                    return View("ViewWriting");
                case ExerciseType.AudioRecording:
                    return View("ViewRecording");
                default:
                    return HttpNotFound(exercise.Type);
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
        // POST: /exercises/save-recording
        [HttpPost]
        public async Task<string> SaveRecording(double duration, string recorderId = null)
        {
            bool success = false;
            try
            {
                using (var stream = Request.InputStream)
                {
                    await UploadUtils.SaveRecording(
                        stream,
                        this.GetUserId(),
                        ExerciseType.AudioRecording,
                        UploadUtils.DurationToLength(duration),
                        null,
                        recorderId // recorderId comes from Audior, 13 digits, milliseconds, current Date in the browser. It is part of Artifact and will be used to update Title with the real title after upload is done.
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
            var host = ConfigurationManager.AppSettings["RecordingTranscoderHost"];
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
                    durationMsec = UploadUtils.GetMp3DurationMsec(stream); // Seeks the stream to the beginning internally.
                }
            }
            // 4. Create a database record.
            var exerciseId = await UploadUtils.CreateExercise(outputBlobName, userId, ExerciseType.AudioRecording, durationMsec, recordingTitle);
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
                        var durationMsec = UploadUtils.GetMp3DurationMsec(stream);
                        if (durationMsec > 0)
                        {
                            var recordingTitle = !String.IsNullOrEmpty(title)
                                ? title
                                : Path.GetFileNameWithoutExtension(mp3File.FileName);

                            exerciseId = await UploadUtils.SaveRecording(
                                stream,
                                userId,
                                ExerciseType.AudioRecording,
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
                    newUserId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.exeTryChangeExerciseAuthor",
                                             new
                                             {
                                                 ExerciseId = exerciseId,
                                                 UserId = userId,
                                                 SkypeName = learnerSkype,
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
        } // end of Upload()

        // POST: /exercises/save-writing-photos
        [HttpPost]
        public async Task<ActionResult> SaveWritingPhotos(IEnumerable<HttpPostedFileBase> files, IEnumerable<int> rotations, string title, string wordCount)
        {
            if (files == null)
            {
                return RedirectToAction("PhotographWriting", new { error = "no_file" });
            }
            if (files.Count() != rotations.Count())
            {
                return RedirectToAction("PhotographWriting", new { error = "wrong_rotations" });
            }

            var items = files
                .Select((i, idx) => new { File = i, Rotation = rotations.ElementAt(idx) })
                // There may be empty form parts from input elements with no file selected.
                .Where(i => i.File != null)
                .Where(i => i.File.ContentType == MediaType.Jpeg);

            var userId = this.GetUserId();
            var userKey = KeyUtils.IntToKey(userId);
            var timeKey = KeyUtils.GetTimeAsBase32();
            var page = 1; // Pages start counting from 1.
            var blobNames = new List<string>();

            foreach (var item in items)
            {
                using (var inputStream = item.File.InputStream)
                {
                    var blobName = String.Format("{0}/{1}/{2}.original.jpg", userKey, timeKey, page);
                    await AzureStorageUtils.UploadBlobAsync(inputStream, AzureStorageUtils.ContainerNames.WritingPhotos, blobName, MediaType.Jpeg);

                    using (var memoryStream = new MemoryStream())
                    {
                        /* I am not sure about using JpegBitmapDecoder. Because of the native code dependencies, the PresentationCore and WindowsBase assemblies need to be distributed as x86 and x64, so AnyCPU may be not possible? */
                        inputStream.Seek(0, SeekOrigin.Begin);
                        using (var image = Image.FromStream(inputStream))
                        {
                            RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                            switch (item.Rotation)
                            {
                                case -1:
                                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                                    break;
                                case 1:
                                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                                    break;
                                default:
                                    break;
                            };
                            if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                            {
                                image.RotateFlip(rotateFlipType);
                            }
                            // We re-encode the image to decrease the size.
                            var codec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            var encoderParams = new EncoderParameters(1);
                            int quality = 50; // Do not pass inline. This parameter is passed via pointer and it has to be strongly typed. Highest quality is 100.
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                            image.Save(memoryStream, codec, encoderParams);
                            //image.Save(memoryStream, ImageFormat.Jpeg);
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        blobName = String.Format("{0}/{1}/{2}.jpg", userKey, timeKey, page);
                        await AzureStorageUtils.UploadBlobAsync(memoryStream, AzureStorageUtils.ContainerNames.WritingPhotos, blobName, MediaType.Jpeg);
                        blobNames.Add(blobName);
                    }

                    page++;
                }
            }

            var artifact = String.Join(",", blobNames);
            int length;
            Int32.TryParse(wordCount, out length); // Assigns 0 if failed.

            var exerciseId = await UploadUtils.CreateExercise(artifact, userId, ExerciseType.WritingPhoto, length, title);

            return RedirectToAction("View", new { Id = exerciseId });
        }

    }
}