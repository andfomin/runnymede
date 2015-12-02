using Runnymede.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.IO;

namespace Runnymede.Common.Utils
{
    public class ExerciseUtils
    {
        public const int MaxExerciseTitleLength = 100; // Corresponds to dbo.exeExercises.Title nvarchar(100)

        public static async Task<IEnumerable<ExerciseDto>> GetExerciseWithReviews(string sql, object param)
        {
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                return await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                    sql,
                    (e, r) => { e.Reviews = new[] { r }; return e; },
                    param,
                    splitOn: "ExerciseId",
                    commandType: CommandType.StoredProcedure
                    );
            }
        }

        public static async Task<IEnumerable<CardDto>> GetCardsWithItems(string sql, object param)
        {
            IEnumerable<CardDto> cards;

            using (var conn = await DapperHelper.GetOpenConnectionAsync())
            {
                cards = await conn.QueryAsync<CardDto, CardItemDto, CardDto>(
                     sql,
                     (c, ci) => { c.Items = new[] { ci }; return c; },
                     param,
                     splitOn: "CardId",
                     commandType: CommandType.Text
                     );
            }

            cards = cards
                .GroupBy(i => i.Id)
                .Select(i =>
                {
                    var items = i
                            .SelectMany(j => j.Items)
                            .OrderBy(j => j.Position)
                            .ToList();
                    var c = i.First();
                    c.Items = items;
                    return c;
                })
                .OrderBy(i => Guid.NewGuid())
                ;

            return cards;
        }

        public static async Task<CardDto> GetCardWithItems(Guid cardId)
        {
            const string sql = @"
select Id, Title, CardId, Position, Content
from dbo.exeCardsWithItems
where Id = @Id;
";
            return (await ExerciseUtils.GetCardsWithItems(sql, new { Id = cardId })).FirstOrDefault();
        }

        /// <summary>
        /// Saves audio recording.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="userId"></param>
        /// <param name="displayName"></param>
        /// <param name="artifactType"></param>
        /// <param name="durationMsec"></param>
        /// <param name="recordingTitle">May come with Skype recording.</param>
        /// <param name="recorderId">Comes from Audior. It is the recorderId and will be used to update Title with the real title after upload is done.</param>
        /// <returns>Exercise Id</returns>
        public static async Task<int> SaveRecording(Stream stream, int userId, string artifactType, string serviceType, int durationMsec, string recordingTitle = null, string recorderId = null)
        {
            var contentType = artifactType == ArtifactType.Mp3 ? MediaType.Mp3 : MediaType.Octet;
            var blobName = UploadUtils.ConstractBlobName(recorderId, userId, contentType);
            await AzureStorageUtils.UploadBlobAsync(stream, AzureStorageUtils.ContainerNames.Recordings, blobName, contentType);
            var exerciseId = await CreateExercise(blobName, userId, serviceType, artifactType, durationMsec, recordingTitle);
            return exerciseId;
        }

        public static async Task<int> CreateExercise(string artifact, int userId, string serviceType, string artifactType, decimal length, 
            string title = null, Guid? cardId = null, string comment = null, string details = null)
        {
            if (String.IsNullOrWhiteSpace(title))
            {
                title = ServiceType.GetTitle(serviceType);
            }

            const string sql = @"
insert dbo.exeExercises (Id, UserId, ServiceType, ArtifactType, Artifact, [Length], Title, CardId, Comment, Details)
output inserted.Id
values (dbo.exeGetNewExerciseId(), @UserId, @ServiceType, @ArtifactType, @Artifact, @Length, @Title, @CardId, @Comment, @Details);
";
            var exerciseId = (await DapperHelper.QueryResilientlyAsync<int>(sql,
                new
                {
                    UserId = userId,
                    Artifact = artifact,
                    ArtifactType = artifactType,
                    ServiceType = serviceType,
                    Length = length,
                    Title = NormalizeExerciseTitle(title),
                    CardId = cardId,
                    Comment = comment,
                    Details = details,
                }))
                .Single();

            return exerciseId;
        }

        public static string NormalizeExerciseTitle(string title)
        {
            return !String.IsNullOrWhiteSpace(title)
                ? (title.Length <= MaxExerciseTitleLength ? title : title.Substring(0, MaxExerciseTitleLength))
                : "Untitled";
        }

        /// <summary>
        /// The directory structure in the "artifacts" Blob Container is "userIdKey/timeKey/fileName.extension" 
        /// </summary>
        /// <param name="userIdKey"></param>
        /// <param name="timeKey"></param>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string FormatBlobName(string userIdKey, string timeKey, string fileName, string extension)
        {
            // Do not use Path.Combine() because Path.DirectorySeparatorChar is '\', whereas CloudBlobClient.DefaultDelimiter is NavigationHelper.Slash .
            return String.Format("{0}/{1}/{2}.{3}", userIdKey, timeKey, fileName, extension);
        }

        //public static string FormatBlobName(int userId, string timeKey, string fileName, string extension)
        //{
        //    return FormatBlobName(KeyUtils.IntToKey(userId), timeKey, fileName, extension);
        //}

        //public static string FormatBlobName(string userIdKey, string timeKey, int index, string extension)
        //{
        //    return FormatBlobName(userIdKey, timeKey, index.ToString(), extension);
        //}

        //public static string FormatBlobName(int userId, string timeKey, int index, string extension)
        //{
        //    return FormatBlobName(KeyUtils.IntToKey(userId), timeKey, index.ToString(), extension);
        //}

    }
}
