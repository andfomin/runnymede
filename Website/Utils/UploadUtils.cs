using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Dapper;
using NAudio.Wave;
using Runnymede.Website.Models;
using System.Data.Entity.SqlServer;
using System.Data;
using ImageResizer;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Threading;
using Runnymede.Common.Utils;

namespace Runnymede.Website.Utils
{

    public static class UploadUtils
    {
        public const int MaxExerciseTitleLength = 100; // Corresponds to dbo.exeExercises.Title nvarchar(100)

        /// <summary>
        /// Saves audio recording.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="userId"></param>
        /// <param name="displayName"></param>
        /// <param name="exerciseType"></param>
        /// <param name="durationMsec"></param>
        /// <param name="recordingTitle">May come with Skype recording.</param>
        /// <param name="recorderId">Comes from Audior. It is the recorderId and will be used to update Title with the real title after upload is done.</param>
        /// <returns>Exercise Id</returns>
        public static async Task<int> SaveRecording(Stream stream, int userId, string exerciseType, int durationMsec, string recordingTitle = null, string recorderId = null)
        {
            var contentType = exerciseType == ExerciseType.AudioRecording ? MediaType.Mp3 : MediaType.Octet;
            var blobName = ConstractBlobName(recorderId, userId, contentType);
            await AzureStorageUtils.UploadBlobAsync(stream, AzureStorageUtils.ContainerNames.Recordings, blobName, contentType);
            var exerciseId = await CreateExercise(blobName, userId, exerciseType, durationMsec, recordingTitle);
            return exerciseId;
        }

        public static async Task<int> CreateExercise(string artifact, int userId, string exerciseType, int length, string title = null, int? cardId = null)
        {
            const string sql = @"
insert dbo.exeExercises (Id, UserId, [Type], Artifact, [Length], Title, CardId)
output inserted.Id
values (dbo.exeGetNewExerciseId(), @UserId, @Type, @Artifact, @Length, @Title, @CardId);
";
            var exerciseId = (await DapperHelper.QueryResilientlyAsync<int>(sql,
                new
                {
                    UserId = userId,
                    Type = exerciseType,
                    Artifact = artifact,
                    Length = length,
                    Title = NormalizeExerciseTitle(title),
                    CardId = cardId,
                }))
                .Single();

            return exerciseId;
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

        public static string NormalizeExerciseTitle(string title)
        {
            return !String.IsNullOrWhiteSpace(title)
                ? (title.Length <= MaxExerciseTitleLength ? title : title.Substring(0, MaxExerciseTitleLength))
                : "Untitled";
        }

        public static int DurationToLength(double duration)
        {
            return Convert.ToInt32(duration * 1000);
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

        public static async Task ResizeAndSaveAvatar(Stream stream, int size, string containerName, string blobName)
        {
            if (stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var destStream = new MemoryStream())
            {
                var instructions = new Instructions()
                {
                    Width = size,
                    Height = size,
                    OutputFormat = OutputFormat.Jpeg,
                    Mode = FitMode.Crop,
                };

                var imageJob = new ImageJob(stream, destStream, instructions);
                imageJob.DisposeSourceObject = false; // Otherwise the stream will be empty on a consecutive read.
                imageJob.Build();

                await AzureStorageUtils.UploadBlobAsync(destStream, containerName, blobName, MediaType.Jpeg);
            }
        }

    }
}