using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dapper;
using System.Data;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Threading;
using System.Data.Entity.SqlServer;
using Microsoft.WindowsAzure.Storage.Table;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/ReviewsApi")]
    public class ReviewsApiController : ApiController
    {

        // GET api/ReviewsApi?offset=1&limit=10
        public DataSourceDto<ReviewDto> GetReviews(int offset, int limit)
        {
            var result = new DataSourceDto<ReviewDto>();

            const string sql = @"
select count(*) from dbo.exeReviews where UserId = @UserId;

select Id, ExerciseId, StartTime, FinishTime, AuthorName, Reward, ExerciseLength
from dbo.exeReviews
where UserId = @UserId
order by StartTime desc
offset @RowOffset rows
fetch next @RowLimit rows only
";
            DapperHelper.QueryMultipleResiliently(
                sql,
                new
                {
                    UserId = this.GetUserId(),
                    RowOffset = offset,
                    RowLimit = limit
                },
                CommandType.Text,
                (Dapper.SqlMapper.GridReader reader) =>
                {
                    result.TotalCount = reader.Read<int>().Single();
                    result.Items = reader.Read<ReviewDto>();
                });

            return result;
        }

        // POST /api/ReviewsApi/
        public async Task<IHttpActionResult> PostReview(JObject value)
        {
            int exerciseId = (int)value["exerciseId"];
            // The current version of GUI allows for selection of a single teacher only. The backend is able to accept an array of teachers.
            var teachersIds = value["teachers"].Values<int>();

            // Parse the reward value entered by the user.
            var rewardStr = ((string)value["reward"]).Trim().Replace(',', '.');
            decimal reward;
            if (!decimal.TryParse(rewardStr, out reward))
                return BadRequest(rewardStr);

            ReviewDto returned = null;

            // We use plain ADO.NET to pass teachers in a table-valued parameter.
            using (var command = new SqlCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.exeCreateReviewRequest";

                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                command.Parameters.AddWithValue("@AuthorUserId", this.GetUserId());
                command.Parameters.AddWithValue("@Reward", reward);

                var teachersRows = new DataTable();
                teachersRows.Columns.Add("UserId", typeof(int));

                foreach (var i in teachersIds)
                {
                    teachersRows.Rows.Add(i);
                }

                var teachersParam = command.Parameters.Add("@ReviewerUserIds", SqlDbType.Structured);
                teachersParam.TypeName = "dbo.appUsersType";
                teachersParam.Value = teachersRows;

                var executionStrategy = new SqlAzureExecutionStrategy();
                await executionStrategy.ExecuteAsync(
                async () =>
                {
                    using (var connection = DapperHelper.GetOpenConnection())
                    {
                        command.Connection = connection;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var results = new List<ReviewDto>();
                            while (reader.Read())
                            {
                                results.Add(new ReviewDto
                                {
                                    Id = (int)reader["Id"],
                                    ExerciseId = (int)reader["ExerciseId"],
                                    Reward = (decimal)reader["Reward"],
                                    RequestTime = (DateTime)reader["RequestTime"],
                                });
                            }
                            returned = results.Single();
                        }
                    }
                },
                new CancellationToken()
                );
            }

            return Content<ReviewDto>(HttpStatusCode.Created, returned);
        }

        // DELETE /api/ReviewsApi/12345
        public async Task<IHttpActionResult> DeleteReviewRequest(int id)
        {
            var cancelTime = (await DapperHelper.QueryResilientlyAsync<DateTime>(
                  "dbo.exeCancelReviewRequest",
                  new { ReviewId = id, UserId = this.GetUserId() },
                  CommandType.StoredProcedure
                  ))
                  .Single();

            return Ok<object>(new { CancelTime = cancelTime });
        }

        // GET /api/ReviewsApi/Requests
        [Route("Requests")]
        public async Task<IEnumerable<object>> GetRequests()
        {
            // There is an index over ReviewerUserId. Hopefully index seek will work.
            var sql = @"
select Id, ReviewId, Reward, AuthorName, TypeId, [Length] 
from dbo.exeRequests
where ReviewerUserId = @ReviewerUserId 
union all
select Id, ReviewId, Reward, AuthorName, TypeId, [Length]
from dbo.exeRequests
where ReviewerUserId is null
";
            var results = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    ReviewerUserId = this.GetUserId(),
                });

            return results;
        }

        // POST /api/ReviewsApi/12345/Start
        [Route("{id:int}/Start")]
        public IHttpActionResult PostStartReview(int id)
        {
            DapperHelper.ExecuteResiliently("dbo.exeStartReview",
                new
                {
                    ReviewId = id,
                    UserId = this.GetUserId(),
                },
                CommandType.StoredProcedure
                );
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/ReviewsApi/Finish/12345
        [Route("Finish/{id:int}")]
        public async Task<IHttpActionResult> PostFinishReview(int id)
        {
            var userId = this.GetUserId();

            // await StatisticsUtils.WriteTagStatistics(id, userId); // It is idempotent. So no transaction is OK.

            var finishTime = (await DapperHelper.QueryResilientlyAsync<DateTime>(
                  "dbo.exeFinishReview",
                  new
                  {
                      ReviewId = id,
                      UserId = userId
                  },
                  CommandType.StoredProcedure
                  ))
                  .Single();

            return Ok<object>(new { FinishTime = finishTime });
        }

        // PUT /api/ReviewsApi/Comment/12345
        [Route("Comment/{id:int}")]
        public IHttpActionResult PutComment(int id, [FromBody]JObject value)
        {
            DapperHelper.ExecuteResiliently("dbo.exeUpdateReviewComment",
            new
            {
                ReviewId = id,
                Comment = (string)value["comment"],
                UserId = this.GetUserId()
            },
            CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/ReviewsApi/Suggestions
        [Route("Suggestions")]
        public async Task<IHttpActionResult> PutSuggestions([FromBody] IEnumerable<SuggestionDto> suggestions)
        {
            // We use plain ADO.NET to pass suggestions in a table-valued parameter.
            using (var command = new SqlCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.exeSaveSuggestions";

                command.Parameters.AddWithValue("@UserId", this.GetUserId());

                var suggestionRows = new DataTable();
                // The order of the columns must match the UDT.
                suggestionRows.Columns.Add("ReviewId", typeof(int));
                suggestionRows.Columns.Add("CreationTime", typeof(int)); // Comes from the client. Means milliseconds passed from the start of the review.
                suggestionRows.Columns.Add("Text", typeof(string));

                foreach (var i in suggestions.OrderBy(i => i.ReviewId).ThenBy(i => i.CreationTime))
                {
                    suggestionRows.Rows.Add(i.ReviewId, i.CreationTime, i.Text);
                }
                // Why performance is actually good is explained at +https://connect.microsoft.com/SQLServer/feedback/details/648637/using-table-valued-parameters-from-clients-cause-recompiles-with-each-use
                var suggestionsParam = command.Parameters.Add("@Suggestions", SqlDbType.Structured);
                suggestionsParam.Direction = ParameterDirection.Input;
                suggestionsParam.TypeName = "dbo.exeSuggestionsType";
                suggestionsParam.Value = suggestionRows;

                var executionStrategy = new SqlAzureExecutionStrategy();
                await executionStrategy.ExecuteAsync(
                async () =>
                {
                    using (var connection = await DapperHelper.GetOpenConnectionAsync())
                    {
                        command.Connection = connection;
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                },
                new CancellationToken()
                );
            }

            return StatusCode(HttpStatusCode.NoContent); // Since we send no content back, 200 OK causes error on the client.
        }

        // DELETE /api/ReviewsApi/Suggestion/12345/12345
        [Route("Suggestion/{reviewId:int}/{creationTime:int}")]
        public async Task<IHttpActionResult> DeleteSuggestion(int reviewId, int creationTime)
        {
            await DapperHelper.ExecuteResilientlyAsync("dbo.exeDeleteSuggestion",
                new
                {
                    ReviewId = reviewId,
                    CreationTime = creationTime,
                    UserId = this.GetUserId()
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
