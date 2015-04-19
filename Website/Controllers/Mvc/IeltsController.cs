using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class IeltsController : Controller
    {
        // GET: Ielts
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult Exercises()
        {
            return View();
        }

        public ActionResult Writing()
        {
            return View();
        }

        public ActionResult Speaking()
        {
            var ua = Request.UserAgent.ToLower();
            var regex = new Regex(".*(android|iphone|ipad|ipod|windows phone).*");
            var mobile = regex.IsMatch(ua);
            return mobile ? View("SpeakingMobile") : View("SpeakingDesktop");
        }

        // POST: /ielts/save-writing-images
        [HttpPost]
        public async Task<ActionResult> SaveWritingImages(IEnumerable<HttpPostedFileBase> files, IEnumerable<int> rotations, string title, int cardId)
        {
            if (files == null || files.All(i => i == null))
            {
                return RedirectToAction("Writing", new { error = "no_file" });
            }
            if (files.Count() != rotations.Count())
            {
                return RedirectToAction("Writing", new { error = "wrong_rotations" });
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

            var exerciseId = await UploadUtils.CreateExercise(artifact, userId, ExerciseType.WritingPhoto, 0, title, cardId);

            return RedirectToAction("View", "Exercises", new { Id = exerciseId });
        }

    }
}