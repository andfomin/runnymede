using Dapper;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    public class StatisticsApiController : ApiController
    {
        // GET api/statisticsapi/getstatistics
        //[Authorize]
        //public IEnumerable<string> GetStatistics(string from, string to)
        //{
        //    if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        //    {
        //        throw new ArgumentNullException();                
        //    }

        //    DateTime f;
        //    DateTime.TryParse(from, out f);

        //    return new [] { f.ToString("u") };
        //}

        // GET api/StatisticsApi/
        public IEnumerable<string> Get(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentNullException(string.Format("from:{0} to:{1}", from, to));
            }

            ////DateTimeOffset fromTime, toTime;
            ////var fromIsValid = DateTimeOffset.TryParse(from, null, DateTimeStyles.AssumeUniversal, out fromTime);
            ////var toIsValid = DateTimeOffset.TryParse(to, null, DateTimeStyles.AssumeUniversal, out toTime);
            DateTime fromTime, toTime;
            var fromIsValid = DateTime.TryParse(from, null, DateTimeStyles.None, out fromTime);
            var toIsValid = DateTime.TryParse(to, null, DateTimeStyles.None, out toTime);

            if (!fromIsValid || !toIsValid)
            {
                throw new ArgumentException(from + "," + to);
            }

            //return new[] { fromTime.ToString("u") };
            // PartitionKey eq '0000000004' and RowKey ge '2013-12-14 20:48:26Z0000000021' and RowKey le '2013-12-14 20:48:26Z0000000021'
            //PartitionKey eq '0000000004' and RowKey ge '2013-12-14 20:48:26Z' and RowKey le '2013-12-14 20:48:26ZA'
            // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 20:48:26Z') and (RowKey le '2013-12-14 20:48:26ZA'))

            // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 15:48:26Z') and (RowKey le '2013-12-14 15:48:26ZA'))

            // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 15:48:26Z') and (RowKey le '2013-12-14 15:48:26ZA'))


            var filterUser = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, AzureStorageUtils.IntToKey(this.GetUserId()));
            var filterFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, AzureStorageUtils.DateTimeToKey(fromTime));
            var filterTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, AzureStorageUtils.DateTimeToKey(toTime) + "A"); // 'A' is greater than any digit in the alphabetical order.
            string combinedDateFilter = TableQuery.CombineFilters(filterFrom, TableOperators.And, filterTo);
            string combinedFilter = TableQuery.CombineFilters(filterUser, TableOperators.And, combinedDateFilter);

            var query = new TableQuery<StatisticsEntity>().Where(combinedFilter);
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.Statistics);
            var entities = table.ExecuteQuery(query);

            // We need to normalize tag count for an exercise according to the review count for that exercise.
            var entitiesWithReviewCount = entities
                .GroupBy(i => i.ExerciseId)
                .Select(i => new { ReviewCount = i.Count(), Entities = i })
                .SelectMany(i => i.Entities, (i, e) => new { ReviewCount = i.ReviewCount, Entity = e });

            var tagCounts = new List<StatisticsEntity.TagCount>(); // Key is ReviewCount.

            // Deserialize tag counts. Tag counts for a review are stored as a single JSON string.
            foreach (var item in entitiesWithReviewCount)
            {
                var tc = JsonConvert.DeserializeObject<List<StatisticsEntity.TagCount>>(item.Entity.TagCounts);
                // We multiply by 1000 to ignore integer division flooring and to avoid float calculations. Actual values do not matter. We do not show them, we use them for sorting, then we take a few top items.
                var normalizedTagCounts = tc.Select(i => new StatisticsEntity.TagCount(i.T, 1000 * i.C / item.ReviewCount));
                tagCounts.AddRange(normalizedTagCounts);
            }

            if (tagCounts.Any())
            {
                var totalTagCounts = tagCounts
                    .GroupBy(i => i.T)
                    .Select(i => new StatisticsEntity.TagCount(i.Key, i.Sum(c => c.C)))
                    .OrderByDescending(i => i.C);                    

                // Display 5 most frequent tags. If some tags have the same Count as the tifth one, include them as well.
                var index = Math.Min(4, totalTagCounts.Count() - 1);
                var threshold = totalTagCounts.ElementAt(index).C;
                var result = totalTagCounts.TakeWhile(i => i.C >= threshold).Select(i => i.T);

                return result;
            }

            return null;
        }

    }
}
