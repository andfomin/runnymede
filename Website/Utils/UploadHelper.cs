using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Dapper;
using NAudio.Wave;
using Runnymede.Website.Controllers;
using Runnymede.Website.Models;
using System.Data.Entity.SqlServer;
using System.Data;

namespace Runnymede.Website.Utils
{
    public static class UploadHelper
    {
        /// <summary>
        /// Saves audio recording.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="userId"></param>
        /// <param name="displayName"></param>
        /// <param name="type"></param>
        /// <param name="durationMsec"></param>
        /// <param name="topicId"></param>
        /// <param name="topicTitle"></param>
        /// <returns></returns>
        public static void SaveRecording(Stream stream, int userId, string type, int durationMsec, string topicId = null, string topicTitle = null)
        {
            //var durationSec = Convert.ToInt32(Math.Round(durationMsec / 1000.0));
            //var durationText = string.Format("{0}:{1}", durationSec / 60, (durationSec % 60).ToString("D2"));

            var maxTitleLength = ExercisesController.MaxExerciseTitleLength;
            string title = !string.IsNullOrEmpty(topicTitle)
                            ? (topicTitle.Length <= maxTitleLength ? topicTitle : topicTitle.Substring(0, maxTitleLength))
                            : "Untitled";

            var artefactId = ControllerHelper.GetTvelveDigitBase32Number();

            // Open the Blob storage
            var blobContainer = AzureStorageUtils.GetCloudBlobContainer(AzureStorageUtils.RecordingsContainerName);
            var blob = blobContainer.GetBlockBlobReference(artefactId);

            blob.Properties.ContentType = type == ExerciseType.AudioRecording ? "audio/mpeg" : "application/octet-stream";

            if (stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            // Save the recording
            blob.UploadFromStream(stream);

            const string sql = @"
insert dbo.exeExercises (UserId, TypeId, ArtefactId, TopicId, [Length], Title)
values (@UserId, @TypeId, @ArtefactId, @TopicId, @Length, @Title);
";
            DapperHelper.ExecuteResiliently(sql,
                new
                {
                    UserId = userId,
                    TypeId = type,
                    ArtefactId = artefactId,
                    TopicId = topicId,
                    Length = durationMsec,
                    Title = title
                });
        }

        //----------------
        public static int GetMp3DurationMsec(Stream stream)
        {
            double durationSec = 0.0;

            MpegVersion mpeg = MpegVersion.Reserved;
            MpegLayer layer = MpegLayer.Reserved;
            ChannelMode mode = ChannelMode.DualChannel;

            try
            {
                if (stream.Position != 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                var frame = Mp3Frame.LoadFromStream(stream);

                if (frame != null)
                {
                    mpeg = frame.MpegVersion;
                    layer = frame.MpegLayer;
                    mode = frame.ChannelMode;
                }

                while (frame != null)
                {
                    // A user can rename a PNG file and try to upload it :) We expect a stable bit pattern in all the frames of a MP3 file.
                    if (frame.MpegVersion != mpeg || frame.MpegLayer != layer || frame.ChannelMode != mode)
                    {
                        return 0;
                    }

                    // 1.0 needed to avoid integer arithmetic and preserve fractions. 
                    durationSec += 1.0 * frame.SampleCount / frame.SampleRate /*(frame.ChannelMode == ChannelMode.Mono ? 1.0 : 1.0)*/;

                    frame = Mp3Frame.LoadFromStream(stream);
                }
            }
            catch (EndOfStreamException)
            {
                // Media players tolerate abruptly terminated MP3 files.
            }

            return Convert.ToInt32(durationSec * 1000);
        }





    }
}