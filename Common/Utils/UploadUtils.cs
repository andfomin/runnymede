//using ImageResizer;
using NAudio.Wave;
using Runnymede.Common.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Runnymede.Common.Utils
{
    public static class UploadUtils
    {
        public static string ConstractArtifactBlobName(int userId, string uniquifier = null, string fileName = null)
        {
            var userKey = KeyUtils.IntToKey(userId);
            uniquifier = String.IsNullOrWhiteSpace(uniquifier)
                ? KeyUtils.GetTwelveBase32Digits()
                : uniquifier.Trim();
            return String.Format("{0}/{1}/{2}", userKey, uniquifier, fileName);
        }

        public static string ConstractBlobName(string recorderId, int userId, string contentType = MediaType.Mp3)
        {
            // recorderId is 13 digits, milliseconds, current Date in the browser.
            var name = !String.IsNullOrWhiteSpace(recorderId)
                ? recorderId.Trim() + userId.ToString()
                : KeyUtils.GetTwelveBase32Digits();
            var extension = MediaType.GetExtension(contentType);
            return Path.ChangeExtension(name, extension);
        }

        public static int DurationToLength(double duration)
        {
            return Convert.ToInt32(duration * 1000);
        }

        //----------------

        //public static async Task ResizeAndSaveAvatar(Stream stream, int size, string containerName, string blobName)
        //{
        //    if (stream.Position != 0)
        //    {
        //        stream.Seek(0, SeekOrigin.Begin);
        //    }

        //    using (var destStream = new MemoryStream())
        //    {
        //        var instructions = new Instructions()
        //        {
        //            Width = size,
        //            Height = size,
        //            OutputFormat = OutputFormat.Jpeg,
        //            Mode = FitMode.Crop,
        //        };

        //        var imageJob = new ImageJob(stream, destStream, instructions);
        //        imageJob.DisposeSourceObject = false; // Otherwise the stream will be empty on a consecutive read.
        //        imageJob.Build();

        //        await AzureStorageUtils.UploadBlobAsync(destStream, containerName, blobName, MediaType.Jpeg);
        //    }
        //}

    }
}