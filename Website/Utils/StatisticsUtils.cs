using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Website.Utils
{
    public class StatisticsUtils
    {

        public static async Task<int> WriteTagStatistics(int reviewId, int userId)
        {
            // The stats calculation should be done within the review finishing procedure before FinishTime is written to the database. This operation is idempotent.
            const string sql = @"
select E.Id, E.UserId, E.CreateTime, E.TypeId, E.[Length]
from dbo.exeExercises E
	inner join dbo.exeReviews R on E.Id = R.ExerciseId
where R.Id = @ReviewId
     and R.UserId = @UserId;
";
            var exercise = (await DapperHelper.QueryResilientlyAsync<ExerciseDto>(sql, new { ReviewId = reviewId, UserId = userId })).Single();

            var tableClient = AzureStorageUtils.GetCloudTableClient();
            var remarksTable = tableClient.GetTableReference(AzureStorageUtils.RemarksTableName);
            var statisticsTable = tableClient.GetTableReference(AzureStorageUtils.StatisticsTableName);

            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, AzureStorageUtils.IntToKey(reviewId));

            var query = new TableQuery<RemarkEntity>()
                .Where(filter)
                .Select(new string[] { "Tags" });

            //var remarks = remarksTable.ExecuteQuery(query); // There is no async counterpart.

            // Execute the query asynchronously.
            List<RemarkEntity> remarks = new List<RemarkEntity>();
            TableQuerySegment<RemarkEntity> currentSegment = null;
            while (currentSegment == null || currentSegment.ContinuationToken != null)
            {
                currentSegment = await remarksTable.ExecuteQuerySegmentedAsync(
                    query,
                    currentSegment != null ? currentSegment.ContinuationToken : null
                    );

                remarks.AddRange(currentSegment.Results);
            }

            var totalRemarkCount = remarks.Count();
            var untaggedRemarkCount = remarks.Count(i => string.IsNullOrWhiteSpace(i.Tags));

            // The Tags field may hold multiple tag values as a CSV. Do not count untagged remarks.
            var tags = remarks
                .SelectMany(i => !string.IsNullOrWhiteSpace(i.Tags) ? i.Tags.Split(',') : new string[] { })
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrEmpty(i)); 

            var tagCounts = tags
                .GroupBy(i => i)
                .Select(i => new StatisticsEntity.TagCount(i.Key, i.Count()))
                .OrderByDescending(i => i.C);

            var totalTagCount = tagCounts.Select(i => i.C).Sum();

            var tagCountsStr = ControllerHelper.SerializeAsJson(tagCounts);

            var entity = new StatisticsEntity
            {
                PartitionKey = AzureStorageUtils.IntToKey(exercise.UserId),
                RowKey = AzureStorageUtils.DateTimeToKey(exercise.CreateTime) + AzureStorageUtils.IntToKey(reviewId), // Make the primary key unique.
                ExerciseId = exercise.Id,
                ReviewId = reviewId,
                ExerciseLength = exercise.Length,
                TotalRemarkCount = totalRemarkCount,
                TotalTagCount = totalTagCount,
                UntaggedRemarkCount = untaggedRemarkCount,
                TagCounts = tagCountsStr,
            };

            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
            await statisticsTable.ExecuteAsync(insertOrReplaceOperation);

            return totalTagCount;
        }



    }
}