using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using Dapper;
using System.Xml.Linq;
using System.Threading;
using System.Text;
using Runnymede.Website.Utils;
using Runnymede.Website.Models;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Runnymede.Website.Controllers
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/ExercisesApi")]
    public class ExercisesApiController : ApiController
    {

        // GET api/ExercisesApi/GetExercises?offset=0&limit=10 
        public DataSourceDto<ExerciseDto> GetExercises(int offset, int limit)
        {
            var result = new DataSourceDto<ExerciseDto>();

            using (var connection = DapperHelper.GetOpenConnection())
            {
//                const string sql = @"
//declare @RowCount int;
//
//set nocount on;
//
//declare @t table (
//	Id int,
//	CreateTime datetime2(0) not null,
//	TypeId nchar(4),
//	Title nvarchar(100),
//	[Length] int
//);
//
//insert into @t (Id, CreateTime, TypeId, Title, [Length])
//	select Id, CreateTime, TypeId, Title, [Length]
//	from dbo.exeExercises
//	where UserId = @UserId;
//
//select @RowCount = count(*) from @t;
//
//delete @t 
//where Id not in 
//	(
//		select Id
//		from @t 
//		order by CreateTime desc
//		offset @RowOffset rows
//		fetch next @RowLimit rows only
//	);
//
//set nocount off;
//
//select Id, CreateTime, TypeId, Title, [Length]
//from @t
//order by CreateTime desc;
//
//select R.ExerciseId, R.Id, R.Reward, R.RequestTime, R.StartTime, R.FinishTime, R.CancelTime, R.ReviewerName 
//from dbo.exeReviews R 
//	inner join @t T on R.ExerciseId = T.Id;
//
//select @RowCount;
//";

                var reader = connection.QueryMultiple(
                    "dbo.exeGetExercises",
                    new { UserId = this.GetUserId(), RowOffset = offset, RowLimit = limit },
                    commandType: CommandType.StoredProcedure
                    );

                result.TotalCount = reader.Read<int>().Single();

                result.Items = reader.Map<ExerciseDto, ReviewDto, int>(e => e.Id, r => r.ExerciseId, (e, r) => { e.Reviews = r; });
            }

            return result;
        }

        // GET /api/ExercisesApi/12345/ReviewConditions
        [Route("{id:int}/ReviewConditions")]
        public async Task<IHttpActionResult> GetReviewConditions(int id)
        {
            var sqlConditions = @"
select dbo.accGetBalance(@UserId) as Balance, dbo.appGetConstantAsFloat('Exercises.Reviews.WorkDurationRatio') as WorkDurationRatio;
";
            var sqlTutors = @"
select Id, DisplayName, Rate 
from dbo.relGetRelatedTutors(@UserId) 
order by DisplayName;
";
            var userId = this.GetUserId();
            dynamic conditions;
            IEnumerable<dynamic> tutors;

            using (var connection = DapperHelper.GetOpenConnection())
            {
                conditions = (await connection.QueryAsync<dynamic>(sqlConditions, new { UserId = userId })).Single();

                tutors = await connection.QueryAsync<dynamic>(sqlTutors, new { UserId = userId });
            }

            return Ok<object>(new
            {
                WorkDurationRatio = (float)conditions.WorkDurationRatio, // Average ratio of work duration to exercise length. It is used for calculation of suggested offers.
                Balance = (decimal)conditions.Balance,
                Tutors = tutors
            });
        }

        ////        // DELETE api/ExercisesApi/123
        ////        public async Task<IHttpActionResult> DeleteExercise(int id)
        ////        {
        ////            // Ensure the user deletes their own exercise.
        ////            var userId = this.GetUserId();
        ////            ExerciseDto exercise;

        ////            using (var connection = DapperHelper.GetOpenConnection())
        ////            {

        ////                string sqlSelect = @"
        ////delete dbo.exeExercises 
        ////	output deleted.TypeId, deleted.ArtefactId
        ////where Id = @Id and UserId = @UserId;
        ////";
        ////                exercise = (await connection.QueryAsync<ExerciseDto>(sqlSelect, new
        ////                   {
        ////                       Id = id,
        ////                       UserId = userId,
        ////                   }))
        ////                   .FirstOrDefault();
        ////            }

        ////            // TypeId is not-nullable, so we expect a row returned back if the row was deleted.
        ////            if (exercise == null)
        ////            {
        ////                return BadRequest();
        ////            }

        ////            // Delete the blob.
        ////            if (!string.IsNullOrEmpty(exercise.ArtefactId))
        ////            {
        ////                if (!string.IsNullOrEmpty(exercise.TypeId) && exercise.TypeId == ExerciseType.AudioRecording)
        ////                {
        ////                    var container = AzureStorageUtils.GetCloudBlobContainer(AzureStorageUtils.RecordingsContainerName);
        ////                    var blob = container.GetBlockBlobReference(exercise.ArtefactId);
        ////                    blob.Delete();
        ////                }
        ////            }

        ////            return StatusCode(HttpStatusCode.NoContent);
        ////        }

        // PUT /api/ExercisesApi/12345/Title
        [Route("{id:int}/Title")]
        public IHttpActionResult PutTitle(int id, [FromBody]JObject value)
        {
            var sql = @"
update dbo.exeExercises set Title = @Title where Id = @Id and UserId = @UserId;
";
            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
            {
                Title = (string)value["title"],
                Id = id,
                UserId = this.GetUserId()
            });
            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        //
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("SaveTopic")]
        public IHttpActionResult SaveTopic(ExerciseSaveTopicModel model)
        {
            var type = string.IsNullOrEmpty(model.Type) ? null : model.Type;
            var title = string.IsNullOrEmpty(model.Title) ? null : model.Title;
            var lines = (model.Lines != null && model.Lines.Any()) ? model.Lines.ToList() : null;

            // Generate Id as a product of the contents.
            // Concatenate the lines.
            // Azure table replaces \r\n with \n when returns string over HTML. Details of that are unclear.
            var allLines = lines != null ? string.Join("\n", lines) : null;

            // A sequence generated by Random is determined by the seed. We need not a trully random ExtId, but a determined and repeatable one based on the contents, a kind of hash.
            var seed1 = ((type ?? "") + (title ?? "") + (allLines ?? "")).GetHashCode();
            var seed2 = ((allLines ?? "") + (title ?? "")).GetHashCode();
            var hashingRandoms = new[] { new Random(seed1), new Random(seed2) };
            var id = ControllerHelper.GetBase32Number(8, hashingRandoms);

            var partitionKey = id;
            var rowKey = title;

            // Find the topic if it already exists.
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TopicsTableName);
            // Prevent exception if not found
            table.ServiceClient.GetTableServiceContext().IgnoreResourceNotFoundException = true;
            var retrieveOperation = TableOperation.Retrieve<TableEntity>(partitionKey, rowKey);
            var tableResult = table.Execute(retrieveOperation);

            // Write if not exists.
            if (tableResult.Result == null)
            {
                var entity = new TopicEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    Type = type,
                    Lines = allLines,
                };
                AzureStorageUtils.InsertEntry(AzureStorageUtils.TopicsTableName, entity);
            }

            return Ok<object>(new { Id = id });
        }


    }
}