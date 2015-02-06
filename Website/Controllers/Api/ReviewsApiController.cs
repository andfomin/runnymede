using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Utils;
using Runnymede.Website.Controllers.Hubs;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/reviews")]
    public class ReviewsApiController : ApiController
    {

        // DELETE /api/reviews/12345
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteReviewRequest(int id)
        {
            await DapperHelper.QueryResilientlyAsync<DateTime>(
                   "dbo.exeCancelReviewRequest",
                   new
                   {
                       UserId = this.GetUserId(),
                       ReviewId = id,
                   },
                   CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/reviews/piece/20000000001/0000000000X0000000000
        [Route("piece/{partitionKey}/{rowKey}")]
        public async Task<IHttpActionResult> DeletePiece(string partitionKey, string rowKey)
        {
            // Check access rights.
            var reviewId = ReviewPiece.GetReviewId(rowKey);
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
            var userIsEditor = await UserIsEditor(partitionKey, reviewId, table);

            if (userIsEditor)
            {
                var entity = new ReviewPiece()
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    ETag = "*",
                };
                var deleteOperation = TableOperation.Delete(entity);
                // If the piece is not yet saved, we will get "The remote server returned an error: (404) Not Found."
                // The approved solution from MS is to try to retrieve the entity first. Catching exception is a hack which is appropriate for a single entity, it is not compatible with a Batch operation.
                try
                {
                    await table.ExecuteAsync(deleteOperation);
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.NotFound)
                        throw;
                }

                // Notify the exercise author in real-time.
                var pieceType = ReviewPiece.GetType(rowKey);
                var pieceId = ReviewPiece.GetPieceId(rowKey);
                this.GetAuthorConnections(partitionKey).PieceDeleted(reviewId, pieceType, pieceId);
            }

            return StatusCode(userIsEditor ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        // GET api/reviews?offset=1&limit=10
        [Route("")]
        public async Task<IHttpActionResult> GetReviews(int offset, int limit)
        {
            return Ok(await DapperHelper.QueryPageItems<ReviewDto>("dbo.exeGetReviews",
                new
                {
                    UserId = this.GetUserId(),
                    RowOffset = offset,
                    RowLimit = limit
                }
                ));
        }

        // GET api/reviews/Exercise/12345/Pieces
        [Route("exercise/{exerciseId:int}/pieces")]
        public async Task<IHttpActionResult> GetAllReviewPieces(int exerciseId)
        {
            var filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ReviewPiece.GetPartitionKey(exerciseId));
            var query = new TableQuery<ReviewPiece>().Where(filterPartition);
            var allPieces = await AzureStorageUtils.ExecuteQueryAsync(AzureStorageUtils.TableNames.ReviewPieces, query);

            // Enforce access rights. The exercise author cannot see review items in an unfinished review. An access entity is written when a review is finished. See ReviewsApiController.PostFinishReview
            var userAccessCode = ReviewPiece.PieceTypes.Viewer + KeyUtils.IntToKey(this.GetUserId());
            // Find the ReviewIds which are allowed to access.
            var reviewIds = allPieces
                .Where(i => ReviewPiece.GetUserAccessCode(i.RowKey) == userAccessCode)
                .Select(i => ReviewPiece.GetReviewId(i.RowKey))
                .ToList();

            RemoveAccessEntries(allPieces);

            // Filter the record set.
            var accessablePieces = allPieces.Where(i => reviewIds.Contains(ReviewPiece.GetReviewId(i.RowKey)));
            var piecesArr = accessablePieces.Select(i => i.Json).ToArray();

            return Ok(piecesArr);
        }

        // GET api/reviews/Exercise/12345/Review/67890/Pieces
        [Route("exercise/{exerciseId:int}/review/{reviewId:int}/pieces")]
        public async Task<IHttpActionResult> GetReviewPieces(int exerciseId, int reviewId)
        {
            var filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ReviewPiece.GetPartitionKey(exerciseId));
            var filterRowFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, KeyUtils.IntToKey(reviewId));
            var filterRowTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, KeyUtils.IntToKey(reviewId) + "Z");
            string combinedRowFilter = TableQuery.CombineFilters(filterRowFrom, TableOperators.And, filterRowTo);
            string combinedFilter = TableQuery.CombineFilters(filterPartition, TableOperators.And, combinedRowFilter);
            var query = new TableQuery<ReviewPiece>().Where(combinedFilter);
            var pieces = await AzureStorageUtils.ExecuteQueryAsync(AzureStorageUtils.TableNames.ReviewPieces, query);

            // The access entity was written at the review start. See ReviewsApiController.PostStartReview
            var userRowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Viewer, this.GetUserId());
            if (pieces.Any(i => i.RowKey == userRowKey))
            {
                RemoveAccessEntries(pieces);
            }
            else
            {
                pieces.Clear();
            }

            var piecesArr = pieces.Select(i => i.Json).ToArray();
            return Ok(piecesArr);
        }

        // GET /api/reviews/requests
        [Route("requests")]
        public async Task<IHttpActionResult> GetRequests()
        {
            var items = await DapperHelper.QueryResilientlyAsync<dynamic>(
                "dbo.exeGetRequests",
                new
                {
                    UserId = this.GetUserId(),
                    UserIsTeacher = this.GetUserIsTeacher(), // If the user is not a teacher, filter out requests to "Any teacher". Return only the direct ones.
                },
                CommandType.StoredProcedure);
            return Ok(new { Items = items });
        }

        // POST /api/reviews/
        [Route("")]
        public async Task<IHttpActionResult> PostRequest(JObject value)
        {
            int exerciseId = (int)value["exerciseId"];

            var reviewers = value["reviewers"]
                .Select(i =>
                {
                    var priceStr = ((string)i["price"]).Trim().Replace(',', '.');
                    decimal price;
                    var parsed = decimal.TryParse(priceStr, out price);
                    return new
                       {
                           UserId = (int?)i["userId"],
                           Price = price,
                           Parsed = parsed
                       };
                })
                .ToList();

            if (reviewers.Any(i => !i.Parsed))
            {
                return BadRequest(value.ToString());
            }

            var requestXml =
                new XElement("Request",
                    new XElement("User", new XAttribute("Id", this.GetUserId())),
                    new XElement("Exercise", new XAttribute("Id", exerciseId)),
                    from r in reviewers
                    select new XElement("Reviewer",
                        r.UserId.HasValue ? new XAttribute("UserId", r.UserId) : null, // Absense of the UserId attribute in T-SQL means UserId = null, i.e. 'Any teacher'.
                        new XAttribute("Price", r.Price)
                        )
                    )
                    .ToString(SaveOptions.DisableFormatting);

            var returned = (await DapperHelper.QueryResilientlyAsync<ReviewDto>(
                            "dbo.exeCreateReviewRequest",
                            new { Request = requestXml },
                            CommandType.StoredProcedure
                            ))
                            .Single();

            return Content<ReviewDto>(HttpStatusCode.Created, returned);
        }

        // POST /api/reviews/12345/start
        [Route("{reviewId:int}/start")]
        public async Task<IHttpActionResult> PostStartReview(int reviewId)
        {
            var userId = this.GetUserId();

            // Get ExerciseId, AuthorUserId, StartTime back.
            var output = (await DapperHelper.QueryResilientlyAsync<dynamic>("dbo.exeStartReview",
                  new
                  {
                      UserId = userId,
                      ReviewId = reviewId,
                  },
                  CommandType.StoredProcedure))
                  .Single();

            /* Write entities which will allow the reviewer to access for reading and writing and the author for reading.
             * We will simply check the presence of one of these records as we read or write the entities.
             * The write entity will be deleted on review finish.
             */
            var partitionKey = ReviewPiece.GetPartitionKey(output.ExerciseId);
            var viewerEntity = new ReviewPiece()
            {
                PartitionKey = partitionKey,
                RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Viewer, userId),
            };
            var editorEntity = new ReviewPiece()
            {
                PartitionKey = partitionKey,
                RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Editor, userId),
            };
            var authorEntity = new ReviewPiece()
            {
                PartitionKey = partitionKey,
                RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Viewer, output.AuthorUserId),
            };

            var batchOperation = new TableBatchOperation();
            batchOperation.InsertOrReplace(viewerEntity);
            batchOperation.InsertOrReplace(editorEntity);
            if (userId != output.AuthorUserId)
            {
                batchOperation.InsertOrReplace(authorEntity);
            }
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
            await table.ExecuteBatchAsync(batchOperation);

            var startTime = DateTime.SpecifyKind(output.StartTime, DateTimeKind.Utc);
            this.GetAuthorConnections(partitionKey).ReviewStarted(reviewId, startTime, output.ReviewerUserId, output.ReviewerName);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/reviews/finish/12345
        [Route("finish/{reviewId:int}")]
        public async Task<IHttpActionResult> PostFinishReview(int reviewId)
        {
            var userId = this.GetUserId();

            // Get ExerciseId and FinishTime back.
            var output = (await DapperHelper.QueryResilientlyAsync<dynamic>(
                "dbo.exeFinishReview",
                new
                {
                    ReviewId = reviewId,
                    UserId = userId,
                },
                CommandType.StoredProcedure))
                .Single();

            var partitionKey = ReviewPiece.GetPartitionKey(output.ExerciseId);
            // Revoke write access from the reviewer.
            var reviewerEntity = new ReviewPiece()
            {
                PartitionKey = partitionKey,
                RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Editor, userId),
                ETag = "*",
            };
            var operation = TableOperation.Delete(reviewerEntity);
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
            await table.ExecuteAsync(operation);

            // Notify the watching exercise author
            var finishTime = DateTime.SpecifyKind(output.FinishTime, DateTimeKind.Utc);
            this.GetAuthorConnections(partitionKey).ReviewFinished(reviewId, finishTime);

            return Ok(new { FinishTime = output.FinishTime });
        }

        // PUT /api/reviews/pieces
        [Route("pieces")]
        public async Task<IHttpActionResult> PutReviewPieces([FromBody] IEnumerable<ReviewPiece> pieces)
        {
            // All pieces must belong to the same exercise and review.
            var partitionKey = pieces
                .Select(i => i.PartitionKey)
                .GroupBy(i => i)
                .Select(i => i.Key)
                .Single();

            var reviewId = pieces
                .Select(i => ReviewPiece.GetReviewId(i.RowKey))
                .GroupBy(i => i)
                .Select(i => i.Key)
                .Single();

            // Ensure that the user is the actual reviewer. Check the presense of the access entry for the user. All pieces must belong to the same exercise and review.
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
            var userIsEditor = await UserIsEditor(partitionKey, reviewId, table);

            if (userIsEditor)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var piece in pieces)
                {
                    batchOperation.InsertOrReplace(piece);
                }
                await table.ExecuteBatchAsync(batchOperation);
            }

            // Notify the exercise author in real-time.
            var piecesArr = pieces.Select(i => i.Json).ToArray();
            this.GetAuthorConnections(partitionKey).PiecesChanged(piecesArr);

            return StatusCode(userIsEditor ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        private async Task<bool> UserIsEditor(string partitionKey, int reviewId, CloudTable table)
        {
            var rowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Editor, this.GetUserId());
            var operation = TableOperation.Retrieve<ReviewPiece>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);
            return result.Result != null;
        }

        private void RemoveAccessEntries(List<ReviewPiece> pieces)
        {
            // Remove access entries from the list.
            pieces.RemoveAll(i =>
            {
                var type = ReviewPiece.GetType(i.RowKey);
                return (type == ReviewPiece.PieceTypes.Editor) || (type == ReviewPiece.PieceTypes.Viewer);
            });
        }

        /// <summary>
        /// The SignalR connections from the exercise author are grouped by ExerciseId.
        /// </summary>
        /// <param name="partitionKey">KeyUtils.IntToKey(exerciseId)</param>
        /// <returns>The dynamic object which can be used to call dynamic methods.</returns>
        private dynamic GetAuthorConnections(string partitionKey)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ReviewHub>();
            return hub.Clients.Group(partitionKey);
        }

    }
}
