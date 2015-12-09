using NAudio.Wave;
using Runnymede.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Common.Utils
{
    public class RecordingTranscoder
    {

        class TrackItem
        {
            public String Name { get; set; }
            public MemoryStream Stream { get; set; }
            public string OriginalFile { get; set; }
            public string IntermidiateFile { get; set; }
            public string Log { get; set; }
        }

        public RecordingTranscoder()
        {
        }

        public async Task<RecordingDetails> Transcode(IEnumerable<KeyValuePair<string, MemoryStream>> tracks, string userIdKey, string timeKey, string extension)
        {
            RecordingDetails result = null;

            // ffmpeg.exe works with files. Produce file paths.
            var workDirPath = GetAppDataDir();
            var fileNamePrefix = userIdKey + "_" + timeKey;

            var trackItems = tracks
                .Select(i => new TrackItem
                {
                    Name = i.Key,
                    Stream = i.Value,
                    OriginalFile = Path.Combine(workDirPath, String.Format("{0}_{1}.{2}", fileNamePrefix, i.Key, extension)),
                    IntermidiateFile = Path.Combine(workDirPath, String.Format("{0}_{1}_intermidiate.mp3", fileNamePrefix, i.Key)),
                })
                .ToList();

            var inputListFilePath = Path.Combine(workDirPath, fileNamePrefix + ".txt");
            var outputFilePath = Path.ChangeExtension(inputListFilePath, ".mp3");

            const string resultFileName = "result";
            var outputBlobName = ExerciseUtils.FormatBlobName(userIdKey, timeKey, resultFileName, "mp3");
            var logBlobName = ExerciseUtils.FormatBlobName(userIdKey, timeKey, resultFileName, "log");

            try
            {
                // Save the original tracks to the disk.
                foreach (var i in trackItems)
                {
                    using (FileStream stream = new FileStream(i.OriginalFile, FileMode.Create, FileAccess.Write))
                    {
                        i.Stream.WriteTo(stream);
                    }
                }

                // Convert to MP3.
                // ffmpeg fails to concatenate AMRs, the error text is misleading "mylistfile.txt: Input/output error". We convert each file separately, then concatenate MP3s.           
                foreach (var i in trackItems)
                {
                    // Increase audio volume by 20dB, convert to MP3 CBR 32kbit/s.
                    var arguments = String.Format("-i \"{0}\" -af \"volume=20dB\" -b:a 32k \"{1}\"", i.OriginalFile, i.IntermidiateFile);
                    i.Log = RunFfmpeg(arguments);
                }

                // Pass the file names to concatenate to ffmpeg.exe in a text file.
                var inputListLines = trackItems.Select(i => String.Format("file '{0}'", i.IntermidiateFile));
                File.WriteAllLines(inputListFilePath, inputListLines);

                // Concatenate MP3s. Do not re-encode, copy existing streams as is.
                var concatArguments = String.Format("-f concat -i \"{0}\" -c copy \"{1}\"", inputListFilePath, outputFilePath);
                var resultLog = RunFfmpeg(concatArguments);

                var separator = Environment.NewLine + "----------------------------------------" + Environment.NewLine;
                var logText = String.Join(separator, trackItems.Select(i => i.Log)) + separator + resultLog;

                var containerName = AzureStorageUtils.ContainerNames.Artifacts;
                var taskMp3 = AzureStorageUtils.UploadFromFileAsync(outputFilePath, containerName, outputBlobName, "audio/mpeg");
                var taskLog = AzureStorageUtils.UploadTextAsync(logText, containerName, logBlobName, "text/plain");
                // Upload the blobs simultaneously.
                await Task.WhenAll(taskMp3, taskLog);

                // Try to read the recording's duration from the ffmpeg logging information.
                bool parsed;
                decimal duration;
                var trackDurations = trackItems
                    .Select((i) =>
                    {
                        parsed = ParseDuration(i.Log, out duration);
                        if (!parsed)
                        {
                            duration = GetMp3Duration(i.IntermidiateFile);
                        }
                        return new KeyValuePair<string, decimal>(i.Name, duration);
                    })
                    .ToDictionary(i => i.Key, i => i.Value)
                    ;

                parsed = ParseDuration(resultLog, out duration);
                if (!parsed)
                {
                    duration = GetMp3Duration(outputFilePath);
                }

                // The JSON encoder with default settings doesn't make upper-case -> lower-case letter conversion of property names. The receiving side is case-sensitive.
                result = new RecordingDetails
                {
                    BlobName = outputBlobName,
                    TotalDuration = duration,
                    TrackDurations = trackDurations,
                };
            }
            finally
            {
                // Clean up the local disk.
                foreach (var i in trackItems)
                {
                    File.Delete(i.OriginalFile);
                    File.Delete(i.IntermidiateFile);
                }
                File.Delete(inputListFilePath);
                File.Delete(outputFilePath);
            }

            return result;
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

        private decimal GetMp3Duration(string filePath)
        {
            // Try to determine the duration directly by reading the file.
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return RecordingUtils.GetMp3Duration(stream);
            }
        }

    }

    public static class RecordingUtils
    {
        public static async Task<decimal> GetMp3Duration(string containerName, string blobName)
        {
            // Read the blob and try to determine the duration directly.
            var outputBlob = AzureStorageUtils.GetBlob(containerName, blobName);
            using (var stream = new MemoryStream())
            {
                await outputBlob.DownloadToStreamAsync(stream);
                return GetMp3Duration(stream); // Seeks the stream to the beginning internally.
            }
        }

        /// <summary>
        /// Reads all frames in MP3. Returns the duration in seconds.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Seconds</returns>
        public static decimal GetMp3Duration(Stream stream)
        {
            double durationSec = 0;

            var version = MpegVersion.Reserved;
            var layer = MpegLayer.Reserved;
            var mode = ChannelMode.DualChannel;

            try
            {
                if (stream.Position != 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                var frame = Mp3Frame.LoadFromStream(stream);

                if (frame != null)
                {
                    version = frame.MpegVersion;
                    layer = frame.MpegLayer;
                    mode = frame.ChannelMode;
                }

                while (frame != null)
                {
                    // A user can rename a PNG file and try to upload it :) We expect a stable bit pattern in all the frames of a MP3 file.
                    if (frame.MpegVersion != version || frame.MpegLayer != layer || frame.ChannelMode != mode)
                    {
                        return 0;
                    }

                    // 1.0m is needed to avoid integer arithmetic and preserve fractions. 
                    durationSec += 1.0 * frame.SampleCount / frame.SampleRate /*(frame.ChannelMode == ChannelMode.Mono ? 1.0 : 1.0)*/;

                    frame = Mp3Frame.LoadFromStream(stream);
                }
            }
            catch (EndOfStreamException)
            {
                // Media players tolerate abruptly terminated MP3 files.
            }

            return Convert.ToDecimal(Math.Round(durationSec, 2));
        }

    }
}
