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

            using (var connection = DapperHelper.GetOpenConnection())
            {
                const string sql = @"
select count(*) from dbo.exeReviews where UserId = @UserId;

select Id, ExerciseId, StartTime, FinishTime, AuthorName, Reward
from dbo.exeReviews
where UserId = @UserId
order by StartTime desc
offset @RowOffset rows
fetch next @RowLimit rows only
";
                var reader = connection.QueryMultiple(
                        sql,
                        new { UserId = this.GetUserId(), RowOffset = offset, RowLimit = limit }
                    );

                result.TotalCount = reader.Read<int>().Single();
                result.Items = reader.Read<ReviewDto>();
            }

            return result;
        }

        // POST /api/ReviewsApi/
        public async Task<IHttpActionResult> PostReview(JObject value)
        {
            int exerciseId = (int)value["exerciseId"];
            var tutorsIds = value["tutors"].Values<int>();

            // Parse the reward value entered by the user.
            var rewardStr = ((string)value["reward"]).Trim().Replace(',', '.');
            decimal reward;
            if (!decimal.TryParse(rewardStr, out reward))
                return BadRequest(rewardStr);

            ReviewDto returned = null;

            // We use plain ADO.NET to pass tutors in a table-valued parameter.
            using (var command = new SqlCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.exeCreateReviewRequest";

                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                command.Parameters.AddWithValue("@AuthorUserId", this.GetUserId());
                command.Parameters.AddWithValue("@Reward", reward);

                var tutorsRows = new DataTable();
                tutorsRows.Columns.Add("UserId", typeof(int));

                tutorsIds.OrderBy(i => i).ToList().ForEach(i =>
                    {
                        tutorsRows.Rows.Add(i);
                    });

                var tutorsParam = command.Parameters.AddWithValue("@ReviewerUserIds", tutorsRows);
                tutorsParam.SqlDbType = SqlDbType.Structured;
                tutorsParam.TypeName = "dbo.appUsersType";

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
        public async Task<IHttpActionResult> DeleteReview(int id)
        {
            var cancelTime = (await DapperHelper.QueryResilientlyAsync<DateTime>(
                  "dbo.exeCancelReviewRequest",
                  new { ReviewId = id, UserId = this.GetUserId() },
                  commandType: CommandType.StoredProcedure
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

        // POST /api/ReviewsApi/12345/Finish
        [Route("{id:int}/Finish")]
        public async Task<IHttpActionResult> PostFinishReview(int id)
        {
            var userId = this.GetUserId();

            await StatisticsUtils.WriteTagStatistics(id, userId); // It is idempotent. So no transaction is OK.

            var finishTime = (await DapperHelper.QueryResilientlyAsync<DateTime>(
                  "dbo.exeFinishReview",
                  new { ReviewId = id, UserId = userId },
                  commandType: CommandType.StoredProcedure
                  ))
                  .Single();

            return Ok<object>(new { FinishTime = finishTime });
        }

        ////        // PUT /api/ReviewsApi/12345/Note
        ////        [Route("{id:int}/Note")]
        ////        public IHttpActionResult PutNote(int id, [FromBody]JObject value)
        ////        {
        ////            var sql = @"
        ////update dbo.exeReviews set Note = @Note where Id = @Id and UserId = @UserId;
        ////";
        ////            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
        ////              {
        ////                  @Note = (string)value["note"],
        ////                  Id = id,
        ////                  UserId = this.GetUserId()
        ////              });
        ////            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        ////        }







    }
}
