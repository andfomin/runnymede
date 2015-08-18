using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Runnymede.Website.Controllers.Mvc
{
    public class ExercisesIeltsController : Runnymede.Website.Utils.CustomController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Speaking()
        {
            var userAgent = Request.UserAgent.ToLower();
            var regex = new Regex(".*(android|iphone|ipad|ipod|windows phone).*");
            var mobile = regex.IsMatch(userAgent);
            return mobile ? View("SpeakingMobile") : View("SpeakingDesktop");
        }

        public ActionResult Writing(string id)
        {
            string serviceType = null;
            switch (id)
            {
                case "task-1":
                    serviceType = ServiceType.IeltsWritingTask1;
                    break;
                case "task-2":
                    serviceType = ServiceType.IeltsWritingTask2;
                    break;
            }

            return View("AnswerSheetPhoto", model: serviceType);
        }

        public ActionResult Reading()
        {
            return View("AnswerSheetPhoto", model: ServiceType.IeltsReading);
        }

        public ActionResult Listening()
        {
            return View("AnswerSheetPhoto", model: ServiceType.IeltsListening);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SavePhotos(IEnumerable<HttpPostedFileBase> files, IEnumerable<int> rotations, string serviceType, Guid? cardId, string title, string comment)
        {
            // Find out the action to redirect to on error. We use the referrer string. RedirectToAction seems accept a case-insensitive parameter.
            var referrerAction = (Request.UrlReferrer.Segments.Skip(2).Take(1).SingleOrDefault() ?? "Index").Trim('/');
            //referrerAction = referrerAction.First().ToString().ToUpper() + referrerAction.Substring(1).ToLower();
            // var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (files == null || files.All(i => i == null))
            {
                return RedirectToAction(referrerAction, new { error = "no_file" });
            }
            if (files.Count() != rotations.Count())
            {
                return RedirectToAction(referrerAction, new { error = "wrong_rotations" });
            }

            var items = files
                .Select((i, idx) => new { File = i, Rotation = rotations.ElementAt(idx) })
                // There may be empty form parts from input elements with no file selected.
                .Where(i => i.File != null);

            if (!items.All(i => i.File.ContentType == MediaType.Jpeg))
            {
                return RedirectToAction(referrerAction, new { error = "wrong_file_format" });
            }

            if (String.IsNullOrWhiteSpace(title))
            {
                title = ServiceType.GetTitle(serviceType);
            }

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
                    await AzureStorageUtils.UploadBlobAsync(inputStream, AzureStorageUtils.ContainerNames.Artifacts, blobName, MediaType.Jpeg);

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
                            // Highest quality is 100. Quality affects the file size. Do not change it until you have exprimented.
                            int quality = 50; // Do not pass inline. This parameter is passed via pointer and it has to be strongly typed. 
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                            image.Save(memoryStream, codec, encoderParams);
                            //image.Save(memoryStream, ImageFormat.Jpeg);
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        blobName = String.Format("{0}/{1}/{2}.jpg", userKey, timeKey, page);
                        await AzureStorageUtils.UploadBlobAsync(memoryStream, AzureStorageUtils.ContainerNames.Artifacts, blobName, MediaType.Jpeg);
                        blobNames.Add(blobName);
                    }

                    page++;
                }
            }

            var artifact = String.Join(",", blobNames);

            var exerciseId = await UploadUtils.CreateExercise(artifact, userId, serviceType, ArtifactType.Jpeg, 0, title, cardId, comment);

            return RedirectToAction("View", "Exercises", new { Id = exerciseId });
        }






        [Authorize]
        [HttpPost]
        // Do not rename the files parameter. The form inputs are bounded by this name.
        public async Task<ActionResult> SaveRecordings(IEnumerable<HttpPostedFileBase> files, string serviceType, Guid? cardId, string title, string comment)
        {
            /*
             * 1. Validate input.
             * 2. Save the original media files to blobs.
             * 3. Call the remote transcoding service. The service converts files to MP3, merges them into a single file, saves it to a blob and returns its name and the recording's duration.
             * 4. Create a database record.
             * 5. Redirect to the exercise page.            
             */

            // 1. Validate input.

            // Find out the action to redirect to on error. We use the referrer string. RedirectToAction seems accept a case-insensitive parameter.
            var referrerAction = (Request.UrlReferrer.Segments.Skip(2).Take(1).SingleOrDefault() ?? "Index").Trim('/');
            //referrerAction = referrerAction.First().ToString().ToUpper() + referrerAction.Substring(1).ToLower();
            // var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (files == null || files.All(i => i == null))
            {
                return RedirectToAction(referrerAction, new { error = "no_file" });
            }

            // There may be empty form parts from input elements with no file selected.
            var originalFiles = files.Where(i => i != null);

            var contentType = originalFiles
                .Select(i => i.ContentType)
                .Distinct()
                .Single()
                ;

            var acceptedContentTypes = (new[] { MediaType.Mpeg, MediaType.Mp3, MediaType.Amr, MediaType.Gpp, MediaType.QuickTime });
            if (! acceptedContentTypes.Contains(contentType))
            {
                return RedirectToAction(referrerAction, new { error = "wrong_file_format" });
            }

            if (String.IsNullOrWhiteSpace(title))
            {
                title = ServiceType.GetTitle(serviceType);
            }

            // 2. Save the original media files to blobs.

            var userId = this.GetUserId(); 
            var userIdKey = KeyUtils.IntToKey(userId);
            var timeKey = KeyUtils.GetTimeAsBase32();
            var index = 0;
            var extension = MediaType.GetExtension(contentType);
            var blobNames = new List<string>();

            foreach (var file in originalFiles)
            {
                using (var inputStream = file.InputStream)
                {
                    // The directory structure in the Blob Storage is userIdKey/timeKey/index.ext. Runnymede.Helper.Controllers.RecordingsController.Get() relies on this structure.
                    var blobName = String.Format("{1}{0}{2}{0}{3}.{4}", AzureStorageUtils.DefaultDirectoryDelimiter, userIdKey, timeKey, index, extension);
                    await AzureStorageUtils.UploadBlobAsync(inputStream, AzureStorageUtils.ContainerNames.Artifacts, blobName, file.ContentType);
                    index++;
                }
            }

            // 3. Call the remote transcoding service.

            var host = ConfigurationManager.AppSettings["RecordingTranscoderHost"];
            var urlFormat = "http://{0}/api/recordings/transcoded/?userIdKey={1}&timeKey={2}&extension={3}&count={4}";
            var url = String.Format(urlFormat, host, userIdKey, timeKey, extension, originalFiles.Count());
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction(referrerAction, new { error = "transcoding_error" });
            }
            // Error is returned as HTML. Then we get error here: No MediaTypeFormatter is available to read an object of type 'JObject' from content with media type 'text/html'.
            var value = await response.Content.ReadAsAsync<JObject>();
            var outputBlobName = (string)value["outputBlobName"];
            var durationMsec = (int)value["durationMsec"];

            // Make sure the duration is known. If the transcoder has failed to parse the ffmpeg logs, it returns DurationMsec = -1.
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
            var exerciseId = await UploadUtils.CreateExercise(outputBlobName, userId, serviceType, ArtifactType.Mp3, durationMsec, title, cardId, comment);

            // 5. Redirect to the exercise page.
            return RedirectToAction("View", "Exercises", new { Id = exerciseId });
        }














    }
}