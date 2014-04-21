using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dapper;
//using Runnymede.Web.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Website.Utils;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/RemarksApi")]
    public class RemarksApiController : ApiController
    {

        // GET api/RemarksApi/Review/12345
        [Route("Review/{reviewId:int}")]
        public IEnumerable<RemarkDto> GetRemarks(int reviewId)
        {
            string partitionKey = AzureStorageUtils.IntToKey(reviewId); // ReviewId is the partition key.
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<RemarkEntity>().Where(filter);
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.Remarks);
            var entities = table.ExecuteQuery(query);

            var result = entities
                .Select(i => new RemarkDto
                        {
                            ReviewId = AzureStorageUtils.KeyToInt(i.PartitionKey),
                            Id = i.RowKey,
                            Start = i.Start,
                            Finish = i.Finish,
                            Text = i.Text,
                            Tags = i.Tags,
                            Starred = i.Starred,
                        }
                );

            return result;
        }

        // DELETE api/RemarksApi/12345/ABCDEF
        [Route("{reviewId}/{remarkId}")]
        public async Task<IHttpActionResult> Delete(int reviewId, string remarkId)
        {
            var entity = new TableEntity()
            {
                PartitionKey = AzureStorageUtils.IntToKey(reviewId),
                RowKey = remarkId,
                ETag = "*"
            };
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.Remarks);
            await table.ExecuteAsync(TableOperation.Delete(entity));
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/RemarksApi/FromEdit
        // PUT /api/RemarksApi/FromView
        [Route("{source}")]
        public async Task<IHttpActionResult> Put(string source, [FromBody] IEnumerable<RemarkDto> remarks)
        {
            IHttpActionResult result;
            switch (source)
            {
                case "FromEdit":
                    result = await PutFromEdit(remarks);
                    break;

                case "FromView":
                    result = await PutFromView(remarks);
                    break;

                default:
                    result = BadRequest("PutEdit" + source);
                    break;
            }
            return result;
        }

        private async Task<IHttpActionResult> PutFromEdit(IEnumerable<RemarkDto> remarks)
        {
            // All the remarks must belong to the same review .
            var reviewId = remarks.GroupBy(i => i.ReviewId).Select(i => i.Key).Single();

            // Ensure that the user is the actual reviewer.
            const string sql = @"
select UserId from dbo.exeReviews where Id = @Id;
";
            var userId = (await DapperHelper.QueryResilientlyAsync<int>(sql, new { Id = reviewId })).SingleOrDefault();

            if (userId != this.GetUserId())
            {
                return BadRequest("UserId");
            }

            var batchOperation = new TableBatchOperation();

            foreach (var remark in remarks)
            {
                // Sanitize the CSV text in the tags field.
                var tags = (string.IsNullOrWhiteSpace(remark.Tags) ? "" : remark.Tags)
                        .Split(',')
                        .Select(i => i.Trim())
                        .Where(i => !string.IsNullOrWhiteSpace(i));

                remark.Tags = tags.Count() > 0 ? string.Join(", ", tags) : null;

                var entity = new RemarkEntity
                {
                    PartitionKey = AzureStorageUtils.IntToKey(reviewId),
                    RowKey = remark.Id,
                    Start = remark.Start,
                    Finish = remark.Finish,
                    Text = remark.Text,
                    Tags = remark.Tags,
                };

                batchOperation.InsertOrReplace(entity);
            }

            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.Remarks);
            await table.ExecuteBatchAsync(batchOperation);

            return StatusCode(HttpStatusCode.NoContent); // Since we send no content back, 200 OK causes error on the JQuery side.
        }

        private async Task<IHttpActionResult> PutFromView(IEnumerable<RemarkDto> remarks)
        {
            // There may be remarks from different reviews in the batch.
            var reviewIds = remarks.GroupBy(i => i.ReviewId).Select(i => i.Key);

            // Ensure that the user is the actual exercise author.
            const string sqlUser = @"
select E.UserId
from dbo.exeReviews R
	inner join dbo.exeExercises E on R.ExerciseId = E.Id
where R.Id in @ReviewIds;
";
            var reviewUsers = await DapperHelper.QueryResilientlyAsync<int>(sqlUser, new { ReviewIds = reviewIds });

            if (reviewUsers.Count() != reviewIds.Count())
            {
                return BadRequest("Count");
            }

            var userId = this.GetUserId();
            if (!reviewUsers.All(i => i == userId))
            {
                return BadRequest("UserId");
            }

            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.Remarks);

            foreach (var remark in remarks)
            {
                var partitionKey = AzureStorageUtils.IntToKey(remark.ReviewId);
                var retrieveOperation = TableOperation.Retrieve<RemarkEntity>(partitionKey, remark.Id);
                var retrievedResult = table.Execute(retrieveOperation);
                var entity = (RemarkEntity)retrievedResult.Result;

                if (entity != null)
                {
                    entity.Starred = remark.Starred;
                    // We cannot use a batch operation becasuse all entities in a batch operation must have the same partition key. But remarks may belong to different reviews.
                    var updateOperation = TableOperation.Replace(entity);
                    table.Execute(updateOperation);
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}

