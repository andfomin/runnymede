using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Runnymede.Recorder.Controllers.Api
{
    [RoutePrefix("api/recordings")]
    public class RecordingsController : ApiController
    {

        // GET api/recordings/app_dir          
        [Route("app_dir")]
        public IHttpActionResult GetAppDir()
        {
            var appDirPath = GeneralUtils.GetAppDataDir();
            return new RawStringResult(this, "app_dir: " + appDirPath, RawStringResult.TextMediaType.PlainText);
        }

        // GET api/recordings/from_freeswitch/?uuid=ae1d4ee2-f4b7-4816-8111-3077e475f771&userId=1234567890    
        [Route("from_freeswitch")]
        public async Task<IHttpActionResult> GetFromFreeswitch(string uuid, int userId = 0)
        {
            RecordingDetails result = null;

            // Produce file paths.
            var workDirPath = GeneralUtils.GetAppDataDir();
            var sourceFilePath = Path.Combine(workDirPath, uuid + ".wav");
            var outputFilePath = Path.ChangeExtension(sourceFilePath, "mp3");

            // The path structure in the Blob Storage is userIdKey/timeKey/filename.ext
            var userIdKey = KeyUtils.IntToKey(userId);
            var timeKey = KeyUtils.GetTimeAsBase32();
            var outputBlobName = String.Format("{0}/{1}/{2}.mp3", userIdKey, timeKey, uuid);
            var logBlobName = Path.ChangeExtension(outputBlobName, "log");

            try
            {
                // Convert to MP3. Increase the audio volume by 10dB, convert to MP3 CBR 64kbit/s.
                var arguments = String.Format("-i \"{0}\" -af \"volume=10dB\" -b:a 64k \"{1}\"", sourceFilePath, outputFilePath);
                var logText = RecordingUtils.RunFfmpeg(arguments);

                var containerName = AzureStorageUtils.ContainerNames.Artifacts;
                var taskMp3 = AzureStorageUtils.UploadFromFileAsync(outputFilePath, containerName, outputBlobName, "audio/mpeg");
                var taskLog = AzureStorageUtils.UploadTextAsync(logText, containerName, logBlobName, "text/plain");
                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Get the recording's duration.
                var duration = RecordingUtils.GetDurationFromFfmpegLogOrMp3File(logText, outputFilePath);

                // Delete the original WAV file on success.
                File.Delete(sourceFilePath);

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new RecordingDetails
                {
                    BlobName = outputBlobName,
                    TotalDuration = duration,
                };
            }
            finally
            {
                // Clean up the MP3 file anyway.
                File.Delete(outputFilePath);
            }

            return Ok<RecordingDetails>(result);
        }

        // We don't use the Web Role anymore.
        //private string GetLocalStoragePath()
        //{
        //    return RoleEnvironment.GetLocalResource("MyLocalStorage").RootPath; 
        //}

        //private string GetExeDirPath()
        //{
        //    // Path.Combine() ignores the first path if it has no trailing back slash.
        //    return Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\", @"approot\bin\");
        //}

    }
}
