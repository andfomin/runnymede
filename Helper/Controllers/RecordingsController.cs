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
using System.Web.Http;

namespace Runnymede.Helper.Controllers
{
    public class RecordingsController : ApiController
    {
        public const string StorageConnectionSetting = "StorageConnection";
        private const string RecordingsBlobContainerSetting = "RecordingsBlobContainer";

        // GET api/recordings/transcoded/?inputBlobName=userIdKey/timeKey/originalFileName.ext
        public async Task<IHttpActionResult> Get(string inputBlobName)
        {
            if (inputBlobName == "endpointMonitoring") {
                return Ok("Endpoint monitoring");
            }

            // 1. Produce file names and paths.
            // "inputBlobName" is expected to be a path like "userIdKey/timeKey/originalFileName.ext"   
            var parts = inputBlobName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() != 3)
            {
                throw new ArgumentException(inputBlobName);
            }
            var userIdKey = parts[0];
            var timeKey = parts[1];
            var originalExtension = Path.GetExtension(parts[2]);
            var newFileName = userIdKey + "_" + timeKey;

            var workDirPath = GetLocalStoragePath();
            var inputFilePath = Path.Combine(workDirPath, newFileName + originalExtension);
            var outputFilePath = Path.ChangeExtension(inputFilePath, ".mp3");

            var outputBlobName = userIdKey + "/" + timeKey + "/transcoded.mp3"; // "userIdKey/timeKey/transcoded.mp3". Do not use Path.Combine() because Path.DirectorySeparatorChar is '\'
            var logBlobName = Path.ChangeExtension(outputBlobName, "log");

            // 2. Process
            var inputBlob = GetBlob(inputBlobName);
            await inputBlob.DownloadToFileAsync(inputFilePath, FileMode.CreateNew);

            var exePath = Path.Combine(GetExeDirPath(), "ffmpeg.exe");
            string arguments = String.Format("-i \"{0}\" \"{1}\"", inputFilePath, outputFilePath);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardError = true,
            };

            var process = System.Diagnostics.Process.Start(processStartInfo);
            // Call of process.StandardError.ReadToEnd() should preceed process.WaitForExit(), not in opposite order, otherwise a dedlock may occur.
            // BTW, do not read StandardOutput stream synchronously, otherwise a dedlock may occur. Details are at the help page for ProcessStartInfo.RedirectStandardError.
            var log = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Close();

            var outputBlob = GetBlob(outputBlobName);
            outputBlob.Properties.ContentType = "audio/mpeg";
            await outputBlob.UploadFromFileAsync(outputFilePath, FileMode.Open);

            var logBlob = GetBlob(logBlobName);
            await logBlob.UploadTextAsync(log);

            File.Delete(inputFilePath);
            File.Delete(outputFilePath);

            // Try to read the recording's duration from the ffmpeg logging information.
            int durationMsec;
            var parsed = ParseDuration(log, out durationMsec);

            var result = new
            {
                outputBlobName = outputBlobName,
                durationMsec = parsed ? durationMsec : -1,
            };
            return Ok(result);
        }

        private CloudBlockBlob GetBlob(string blobName)
        {
            // +http://www.heikniemi.net/hardcoded/2013/06/encrypting-connection-strings-in-windows-azure-web-applications/
            // RoleEnvironment.IsEmulated
            //var connectionString = ConfigurationManager.ConnectionStrings[StorageConnectionStringName].ConnectionString;
            //var containerName = ConfigurationManager.AppSettings[RecordingsBlobContainerName];
            var connectionString = RoleEnvironment.GetConfigurationSettingValue(StorageConnectionSetting);
            var containerName = RoleEnvironment.GetConfigurationSettingValue(RecordingsBlobContainerSetting);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            return blob;
        }

        private string GetLocalStoragePath()
        {
            return RoleEnvironment.GetLocalResource("MyLocalStorage").RootPath;
        }

        private string GetExeDirPath()
        {
            // Path.Combine() ignores the first path without the trailing "\".
            return Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\", @"approot\bin\");
        }

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
