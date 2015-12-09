using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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

namespace Runnymede.Helper.Controllers
{
    [RoutePrefix("api/recordings")]
    public class RecordingsController : ApiController
    {
        public const string StorageConnectionSetting = "StorageConnection";
        private const string RecordingsBlobContainerSetting = "RecordingsBlobContainer";

        //// GET api/recordings/transcoded/?inputBlobName=userIdKey/timeKey/originalFileName.ext
        //public async Task<IHttpActionResult> Get0(string inputBlobName)
        //{
        //    if (inputBlobName == "endpointMonitoring") {
        //        return Ok("Endpoint monitoring");
        //    }

        //    // 1. Produce file names and paths.
        //    // "inputBlobName" is expected to be a path like "userIdKey/timeKey/originalFileName.ext"   
        //    var parts = inputBlobName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        //    if (parts.Count() != 3)
        //    {
        //        throw new ArgumentException(inputBlobName);
        //    }
        //    var userIdKey = parts[0];
        //    var timeKey = parts[1];
        //    var originalExtension = Path.GetExtension(parts[2]);
        //    var newFileName = userIdKey + "_" + timeKey;

        //    var workDirPath = GetLocalStoragePath();
        //    var inputFilePath = Path.Combine(workDirPath, newFileName + originalExtension);
        //    var outputFilePath = Path.ChangeExtension(inputFilePath, ".mp3");

        //    var outputBlobName = userIdKey + "/" + timeKey + "/transcoded.mp3"; // "userIdKey/timeKey/transcoded.mp3". Do not use Path.Combine() because Path.DirectorySeparatorChar is '\'
        //    var logBlobName = Path.ChangeExtension(outputBlobName, "log");

        //    // 2. Process
        //    var inputBlob = GetBlob(inputBlobName);
        //    await inputBlob.DownloadToFileAsync(inputFilePath, FileMode.CreateNew);

        //    var exePath = Path.Combine(GetExeDirPath(), "ffmpeg.exe");
        //    string arguments = String.Format("-i \"{0}\" \"{1}\"", inputFilePath, outputFilePath);
        //    var processStartInfo = new ProcessStartInfo
        //    {
        //        FileName = exePath,
        //        Arguments = arguments,
        //        CreateNoWindow = true,
        //        UseShellExecute = false,
        //        ErrorDialog = false,
        //        RedirectStandardError = true,
        //    };

        //    var process = System.Diagnostics.Process.Start(processStartInfo);
        //    // Call of process.StandardError.ReadToEnd() should preceed process.WaitForExit(), not in opposite order, otherwise a dedlock may occur.
        //    // BTW, do not read StandardOutput stream synchronously, otherwise a dedlock may occur. Details are at the help page for ProcessStartInfo.RedirectStandardError.
        //    var log = process.StandardError.ReadToEnd();
        //    process.WaitForExit();
        //    process.Close();

        //    var outputBlob = GetBlob(outputBlobName);
        //    outputBlob.Properties.ContentType = "audio/mpeg";
        //    await outputBlob.UploadFromFileAsync(outputFilePath, FileMode.Open);

        //    var logBlob = GetBlob(logBlobName);
        //    await logBlob.UploadTextAsync(log);

        //    File.Delete(inputFilePath);
        //    File.Delete(outputFilePath);

        //    // Try to read the recording's duration from the ffmpeg logging information.
        //    int durationMsec;
        //    var parsed = ParseDuration(log, out durationMsec);

        //    var result = new
        //    {
        //        outputBlobName = outputBlobName,
        //        durationMsec = parsed ? durationMsec : -1,
        //    };
        //    return Ok(result);
        //}
        /*
         */

        // GET api/recordings/transcoded/?userIdKey=1234567890&timeKey=qwe123asd&extension=amr&count=19            
        [Route("transcoded")]
        public async Task<IHttpActionResult> GetTranscoded(string userIdKey = null, string timeKey = null, string extension = null, int count = 0)
        {
            if (count < 1)
            {
                return Ok("Endpoint monitoring " + DateTime.UtcNow.ToString("u"));
            }

            dynamic result = null;

            // Produce file paths.

            var blobContainer = GetBlobContainer();
            // The directory structure in the Blob Storage is userIdKey/timeKey/index.ext
            // Do not use Path.Combine() because Path.DirectorySeparatorChar is '\', whereas CloudBlobClient.DefaultDelimiter is NavigationHelper.Slash .
            var blobFolder = userIdKey + "/" + timeKey;
            var outputBlobName = blobFolder + "/transcoded.mp3";
            var logBlobName = Path.ChangeExtension(outputBlobName, "log");

            var workDirPath = GetAppDataDir(); // GetLocalStoragePath();
            var fileNamePrefix = userIdKey + "_" + timeKey;

            var filePaths = Enumerable.Range(0, count)
                .Select(i => new
                {
                    BlobName = String.Format("{0}/{1}.{2}", blobFolder, i, extension),
                    Original = Path.Combine(workDirPath, String.Format("{0}_{1}.{2}", fileNamePrefix, i, extension)),
                    Intermidiate = Path.Combine(workDirPath, String.Format("{0}_{1}_inter.mp3", fileNamePrefix, i)),
                });

            var inputListFilePath = Path.Combine(workDirPath, fileNamePrefix + "txt");
            var outputFilePath = Path.ChangeExtension(inputListFilePath, ".mp3");

            try
            {
                // Download all blobs in parallel.
                var tasks = filePaths.Select(i =>
                {
                    var blob = blobContainer.GetBlockBlobReference(i.BlobName);
                    return blob.DownloadToFileAsync(i.Original, FileMode.CreateNew);
                });
                await Task.WhenAll(tasks);

                string arguments;
                var logs = new List<string>();

                // Convert to MP3.
                // ffmpeg fails to concatenate AMRs, the error text is misleading "mylistfile.txt: Input/output error". We convert each file separately, then concatenate MP3s.           
                foreach (var i in filePaths)
                {
                    // Increase audio volume by 20dB, convert to MP3 CBR 32kbit/s.
                    arguments = String.Format("-i \"{0}\" -af \"volume=20dB\" -b:a 64k \"{1}\"", i.Original, i.Intermidiate);
                    var logText1 = RunFfmpeg(arguments);
                    logs.Add(logText1);
                }

                // Pass the file names to concatenate to ffmpeg.exe in a text file.
                var inputListLines = filePaths.Select(i => String.Format("file '{0}'", i.Intermidiate));
                File.WriteAllLines(inputListFilePath, inputListLines);

                // Concatenate MP3s. Do not re-encode, copy existing streams as is.
                arguments = String.Format("-f concat -i \"{0}\" -c copy \"{1}\"", inputListFilePath, outputFilePath);
                var logText2 = RunFfmpeg(arguments);
                logs.Add(logText2);

                var outputBlob = blobContainer.GetBlockBlobReference(outputBlobName);
                outputBlob.Properties.ContentType = "audio/mpeg";
                var taskMp3 = outputBlob.UploadFromFileAsync(outputFilePath, FileMode.Open);

                var logBlob = blobContainer.GetBlockBlobReference(logBlobName);
                logBlob.Properties.ContentType = "text/plain";
                var logText = String.Join(Environment.NewLine + "---------------------------" + Environment.NewLine, logs);
                var taskLog = logBlob.UploadTextAsync(logText);

                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Try to read the recording's duration from the ffmpeg logging information.
                int durationMsec;
                var parsed = ParseDuration(logText2, out durationMsec);

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new
                {
                    outputBlobName = outputBlobName,
                    durationMsec = parsed ? durationMsec : -1,
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

            return Ok(result);
        }

        // GET api/recordings/freeswitch/?uuid=ae1d4ee2-f4b7-4816-8111-3077e475f771&userIdKey=1234567890&timeKey=qwe123asd           
        [Route("freeswitch")]
        public async Task<IHttpActionResult> GetFromFreeswitch(string uuid, string userIdKey = null, string timeKey = null)
        {
            dynamic result = null;

            // Produce file paths.
            var workDirPath = GetAppDataDir();
            var sourceFilePath = Path.Combine(workDirPath, uuid + ".wav");
            var outputFilePath = Path.ChangeExtension(sourceFilePath, "mp3");

            var blobContainer = GetBlobContainer();
            // The directory structure in the Blob Storage is userIdKey/timeKey/filename.ext
            // Do not use Path.Combine() because Path.DirectorySeparatorChar is '\', whereas CloudBlobClient.DefaultDelimiter is NavigationHelper.Slash .
            var blobFolder = userIdKey + "/" + timeKey;
            var outputBlobName = blobFolder + "/fs.mp3";
            var logBlobName = Path.ChangeExtension(outputBlobName, "log");

            try
            {
                // Convert to MP3. Increase the audio volume by 10dB, convert to MP3 CBR 64kbit/s.
                var arguments = String.Format("-i \"{0}\" -af \"volume=10dB\" -b:a 64k \"{1}\"", sourceFilePath, outputFilePath);
                var logText = RunFfmpeg(arguments);

                var outputBlob = blobContainer.GetBlockBlobReference(outputBlobName);
                outputBlob.Properties.ContentType = "audio/mpeg";
                var taskMp3 = outputBlob.UploadFromFileAsync(outputFilePath, FileMode.Open);

                var logBlob = blobContainer.GetBlockBlobReference(logBlobName);
                logBlob.Properties.ContentType = "text/plain";
                var taskLog = logBlob.UploadTextAsync(logText);

                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Try to read the recording's duration from the ffmpeg logging information.
                int durationMsec;
                var parsed = ParseDuration(logText, out durationMsec);

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new
                {
                    outputBlobName = outputBlobName,
                    durationMsec = parsed ? durationMsec : -1,
                };

                // Delete the original WAV file on success.
                File.Delete(sourceFilePath);
            }
            finally
            {
                // Clean up the MP3 file.
                File.Delete(outputFilePath);
            }

            return Ok(result);
        }

        private string RunFfmpeg(string arguments)
        {
            // Path.Combine() ignores the first path part if it has no trailing back slash.
            //var exeDirPath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\", @"approot\bin\");
            var exeDirPath = GetAppDataDir();
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

        private CloudBlobContainer GetBlobContainer()
        {
            // +http://www.heikniemi.net/hardcoded/2013/06/encrypting-connection-strings-in-windows-azure-web-applications/
            // RoleEnvironment.IsEmulated
            //var connectionString = RoleEnvironment.GetConfigurationSettingValue(StorageConnectionSetting);
            //var containerName = RoleEnvironment.GetConfigurationSettingValue(RecordingsBlobContainerSetting);
            var connectionString = ConfigurationManager.ConnectionStrings[StorageConnectionSetting].ConnectionString;
            var containerName = ConfigurationManager.AppSettings[RecordingsBlobContainerSetting];
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            return blobContainer;
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

        private bool ParseDuration(string log, out int durationMsec)
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
            durationMsec = success ? Convert.ToInt32(timeSpan.TotalMilliseconds) : -1;
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
