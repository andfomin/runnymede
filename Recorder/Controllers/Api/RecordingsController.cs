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
            var appDirPath = GetAppDataDir();
            return new RawStringResult(this, "app_dir: " + appDirPath, RawStringResult.TextMediaType.PlainText);
        }

        // GET api/recordings/transcoded/?userIdKey=1234567890&timeKey=qwe123asd&extension=mov&fileNames=1a,1b,2a,3a            
        [Route("transcoded")]
        public async Task<IHttpActionResult> GetTranscoded(string userIdKey = null, string timeKey = null, string extension = null, string fileNames = null)
        {
            if (String.IsNullOrEmpty(userIdKey))
            {
                return new RawStringResult(this, "Endpoint monitoring " + DateTime.UtcNow.ToString("u"), RawStringResult.TextMediaType.PlainText);
                //return Ok("Endpoint monitoring " + DateTime.UtcNow.ToString("u")); // Returns XML
            }

            RecordingDetails result = null;

            // Produce file paths.
            const string resultFileName = "result";
            var outputBlobName = ExerciseUtils.FormatBlobName(userIdKey, timeKey, resultFileName, "mp3");
            var logBlobName = ExerciseUtils.FormatBlobName(userIdKey, timeKey, resultFileName, "log");

            var workDirPath = GetAppDataDir(); // GetLocalStoragePath();
            var fileNamePrefix = userIdKey + "_" + timeKey;

            var filePaths = fileNames.Split(',')
                .Select(i => new
                {
                    FileName = i,
                    BlobName = ExerciseUtils.FormatBlobName(userIdKey, timeKey, i, extension),
                    Original = Path.Combine(workDirPath, String.Format("{0}_{1}.{2}", fileNamePrefix, i, extension)),
                    Intermidiate = Path.Combine(workDirPath, String.Format("{0}_{1}_inter.mp3", fileNamePrefix, i)),
                });

            var inputListFilePath = Path.Combine(workDirPath, fileNamePrefix + ".txt");
            var outputFilePath = Path.ChangeExtension(inputListFilePath, ".mp3");

            try
            {
                var blobContainer = AzureStorageUtils.GetCloudBlobContainer(AzureStorageUtils.ContainerNames.Artifacts);
                // Download all blobs in parallel.
                var tasks = filePaths.Select(i =>
                {
                    var blob = blobContainer.GetBlockBlobReference(i.BlobName);
                    return blob.DownloadToFileAsync(i.Original, FileMode.CreateNew);
                });
                await Task.WhenAll(tasks);

                string arguments;
                var trackLogs = new Dictionary<string, string>();

                // Convert to MP3.
                // ffmpeg fails to concatenate AMRs, the error text is misleading "mylistfile.txt: Input/output error". We convert each file separately, then concatenate MP3s.           
                foreach (var i in filePaths)
                {
                    // Increase audio volume by 20dB, convert to MP3 CBR 32kbit/s.
                    arguments = String.Format("-i \"{0}\" -af \"volume=20dB\" -b:a 32k \"{1}\"", i.Original, i.Intermidiate);
                    var log = RunFfmpeg(arguments);
                    trackLogs.Add(i.FileName, log);
                }

                // Pass the file names to concatenate to ffmpeg.exe in a text file.
                var inputListLines = filePaths.Select(i => String.Format("file '{0}'", i.Intermidiate));
                File.WriteAllLines(inputListFilePath, inputListLines);

                // Concatenate MP3s. Do not re-encode, copy existing streams as is.
                arguments = String.Format("-f concat -i \"{0}\" -c copy \"{1}\"", inputListFilePath, outputFilePath);
                var resultLog = RunFfmpeg(arguments);

                var separator = Environment.NewLine + "----------------------------------------" + Environment.NewLine;
                var logText = String.Join(separator, trackLogs) + separator + resultLog;

                var containerName = AzureStorageUtils.ContainerNames.Artifacts;
                var taskMp3 = AzureStorageUtils.UploadFromFileAsync(outputFilePath, containerName, outputBlobName, "audio/mpeg");
                var taskLog = AzureStorageUtils.UploadTextAsync(logText, containerName, logBlobName, "text/plain");
                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Try to read the recording's duration from the ffmpeg logging information.
                bool parsed;
                decimal duration;
                var trackDurations = trackLogs
                    .Select((i) =>
                    {
                        parsed = ParseDuration(i.Value, out duration);
                        return new KeyValuePair<string, decimal>(i.Key, parsed ? duration : 0);
                    })
                    .ToDictionary(i => i.Key, i=> i.Value)
                    ;
                parsed = ParseDuration(resultLog, out duration);

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new RecordingDetails
                {
                    BlobName = outputBlobName,
                    TotalDuration = parsed ? duration : 0,
                    TrackDurations = trackDurations,
                };

            }
            finally
            {
                // Clean up the local disk.
                foreach (var i in filePaths)
                {
                    File.Delete(i.Original);
                    File.Delete(i.Intermidiate);
                }
                File.Delete(inputListFilePath);
                File.Delete(outputFilePath);
            }

            return Ok<RecordingDetails>(result);
        }

        // GET api/recordings/from_freeswitch/?uuid=ae1d4ee2-f4b7-4816-8111-3077e475f771&userId=1234567890        
        [Route("from_freeswitch")]
        public async Task<IHttpActionResult> GetFromFreeswitch(string uuid, int userId = 0)
        {
            RecordingDetails result = null;

            // Produce file paths.
            var workDirPath = GetAppDataDir();
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
                var logText = RunFfmpeg(arguments);

                var containerName = AzureStorageUtils.ContainerNames.Artifacts;
                var taskMp3 = AzureStorageUtils.UploadFromFileAsync(outputFilePath, containerName, outputBlobName, "audio/mpeg");
                var taskLog = AzureStorageUtils.UploadTextAsync(logText, containerName, logBlobName, "text/plain");
                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Try to read the recording's duration from the ffmpeg logging information.
                decimal duration;
                var parsed = ParseDuration(logText, out duration);

                // Delete the original WAV file on success.
                File.Delete(sourceFilePath);

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new RecordingDetails
                {
                    BlobName = outputBlobName,
                    TotalDuration = parsed ? duration : 0,
                };
            }
            finally
            {
                // Clean up the MP3 file anyway.
                File.Delete(outputFilePath);
            }

            return Ok<RecordingDetails>(result);
        }

        private string RunFfmpeg(string arguments)
        {
            // Path.Combine() ignores the first path part if it has no trailing back slash.
            //var exeDirPath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\", @"approot\bin\");
            //var exeDirPath = GetAppDataDir();
            var exeDirPath = HttpRuntime.BinDirectory;
            var exePath = Path.Combine(exeDirPath, "ffmpeg.exe");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardError = true,
            };
            var process = Process.Start(processStartInfo);
            // Call of process.StandardError.ReadToEnd() should preceed process.WaitForExit(), not in opposite order, otherwise a dedlock may occur.
            // BTW, do not read StandardOutput stream synchronously, otherwise a dedlock may occur. Details are at the help page for ProcessStartInfo.RedirectStandardError.
            var logText = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return logText;
        }

        private string GetAppDataDir()
        {
            return HttpContext.Current.Server.MapPath("~/App_Data");
        }

        // We don't use Web Role anymore.
        //private string GetLocalStoragePath()
        //{
        //    return RoleEnvironment.GetLocalResource("MyLocalStorage").RootPath; 
        //}

        //private string GetExeDirPath()
        //{
        //    // Path.Combine() ignores the first path if it has no trailing back slash.
        //    return Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\", @"approot\bin\");
        //}

        private bool ParseDuration(string log, out decimal durationSec)
        {
            var lines = ReadLines(log);
            var line = lines
                .Where(i => i.StartsWith("size=") && i.Contains("time=") && i.Contains("bitrate="))
                .Select(i =>
                {
                    var start = i.IndexOf("time=") + 5;
                    var length = i.IndexOf("bitrate=") - start;
                    if (length < 0)
                    {
                        return "";
                    }
                    var snippet = i.Substring(start, length).Trim();
                    return snippet;
                })
                .FirstOrDefault()
            ;
            TimeSpan timeSpan;
            var success = TimeSpan.TryParseExact(line, "c", null, out timeSpan);
            durationSec = success ? Decimal.Round(Convert.ToDecimal(timeSpan.TotalMilliseconds / 1000), 2) : 0;
            return success;
        }

        private IEnumerable<string> ReadLines(string s)
        {
            // StringReader.ReadLine() recognizes a line feed ("\n"), a carriage return ("\r"), or a carriage return immediately followed by a line feed ("\r\n"). 
            string line;
            using (var sr = new StringReader(s))
                while ((line = sr.ReadLine()) != null)
                    yield return line;
        }

    }
}
