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
using System.IO;
using System.Net.Http.Headers;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/exercises")]
    public class ExercisesApiController : ApiController
    {
        // GET api/exercises/?offset=0&limit=10&type=WRPH
        [Route("")]
        public async Task<IHttpActionResult> GetExercises(int offset, int limit, string type)
        {
            object result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                  "dbo.exeGetExercises",
                  new
                  {
                      UserId = this.GetUserId(),
                      Type = type,
                      RowOffset = offset,
                      RowLimit = limit,
                  },
                  CommandType.StoredProcedure,
                  (Dapper.SqlMapper.GridReader reader) =>
                  {
                      result = new
                      {
                          Items = reader.Map<ExerciseDto, ReviewDto, int>(e => e.Id, r => r.ExerciseId, (e, r) => { e.Reviews = r; }),
                          TotalCount = reader.Read<int>().Single(),
                      };
                  });

            return Ok(result);
        }

        // GET /api/exercises/EX____/review_conditions
        [Route("{exerciseType:alpha}/review_conditions")]
        public async Task<IHttpActionResult> GetReviewConditions(string exerciseType)
        {
            string fieldName = "null";
            switch (exerciseType)
            {
                case ExerciseType.AudioRecording:
                    fieldName = "RecordingRate";
                    break;
                case ExerciseType.WritingPhoto:
                    fieldName = "WritingRate";
                    break;
                default:
                    break;
            }

            var sql = @"
select dbo.accGetBalance(@UserId);
select dbo.exeGetAnyTeacherReviewRate(@ExerciseType);
" + String.Format("select Id, DisplayName, {0} as Rate from dbo.friGetReviewers(@UserId) where {0} is not null;", fieldName);

            object result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                sql,
                new
                {
                    UserId = this.GetUserId(),
                    ExerciseType = exerciseType
                },
                CommandType.Text,
                (Dapper.SqlMapper.GridReader reader) =>
                {
                    result = new
                      {
                          Balance = reader.Read<decimal?>().Single(),
                          AnyTeacherReviewRate = reader.Read<decimal>().Single(),
                          Reviewers = reader.Read<dynamic>().ToList(),
                      };
                });
            return Ok(result);
        }

        // No need for async. This method is called only on the development machine. The production code downloads from Blob directly.
        // GET api/exercises/artifact/writing-photos?blobName=0000000003/22273p29f8/2.jpg
        [Route("artifact/{containerName}")]
        public HttpResponseMessage GetArtifact(string containerName, string blobName)
        {
            var blob = AzureStorageUtils.GetBlob(containerName, blobName);
            blob.FetchAttributes();
            var contentType = blob.Properties.ContentType;
            var ms = new System.IO.MemoryStream();
            blob.DownloadToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(ms);
            // ctor MediaTypeHeaderValue() does not accept "text/plain; charset=utf-8" directly
            MediaTypeHeaderValue value;
            if (MediaTypeHeaderValue.TryParse(contentType, out value))
            {
                result.Content.Headers.ContentType = value;
            }
            return result;
        }

        // PUT /api/exercises/12345/title
        [Route("{id:int}/title")]
        public async Task<IHttpActionResult> PutTitle(int id, [FromBody]JObject value)
        {
            var sql = @"
update dbo.exeExercises 
set Title = @Title 
where Id = @Id and UserId = @UserId;
";
            var rowsAffected = await DapperHelper.ExecuteResilientlyAsync(sql, new
            {
                Title = (string)value["title"],
                Id = id,
                UserId = this.GetUserId()
            });
            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        // PUT /api/exercises/recording_title
        [Route("recording_title")]
        public async Task<IHttpActionResult> PutRecordingTitle([FromBody]JObject value)
        {
            var recorderId = (string)value["recorderId"];
            // Execute the query even if the title is null, we need to send the ExerciseId back.
            var exerciseId = 0;
            if (!String.IsNullOrWhiteSpace(recorderId))
            {
                var userId = this.GetUserId();

                var sql = @"
update dbo.exeExercises
set Title = @Title 
output deleted.Id
where UserId = @UserId 
    and [Length] = @Length 
    and Artifact = @Artifact;
";
                exerciseId = (await DapperHelper.QueryResilientlyAsync<int>(sql,
                    new
                    {
                        Title = UploadUtils.NormalizeExerciseTitle((string)value["title"]),
                        UserId = userId,
                        Length = UploadUtils.DurationToLength((double)value["duration"]), // We use duration because there is a chance that the same instance of recorder may be reused.
                        Artifact = UploadUtils.ConstractBlobName(recorderId, userId),
                    }))
                    .Single();
            }

            return Ok(new { ExerciseId = exerciseId });
        }

    }
}