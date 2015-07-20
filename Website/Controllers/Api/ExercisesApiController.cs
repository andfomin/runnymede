using Dapper;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/exercises")]
    public class ExercisesApiController : ApiController
    {
        // GET api/exercises/?offset=0&limit=10
        [Route("")]
        public async Task<IHttpActionResult> GetExercises(int offset, int limit)
        {
            object result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                  "dbo.exeGetExercises",
                  new
                  {
                      UserId = this.GetUserId(),
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

        // GET /api/exercises/1234567890/review_conditions
        [Route("{id:int}/review_conditions")]
        public async Task<IHttpActionResult> GetReviewConditions(int id)
        {
            const string sql = @"
select dbo.accGetBalance(E.UserId) as Balance, dbo.appGetServicePrice(E.ServiceType) as Price
from dbo.exeExercises E
where E.Id = @ExerciseId
	and E.UserId = @UserId;
";
            // Returns null for Balance the user has not created the account yet.
            var data = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new
            {
                UserId = this.GetUserId(),
                ExerciseId = id,
            }
            )).SingleOrDefault();
            return Ok(data);
        }

        // No need for async. This method is called only on the development machine. The production code downloads from Blob directly.
        // GET api/exercises/artifact/writing-photos?blobName=0000000003/22273p29f8/2.jpg
        [Route("artifact/{containerName}")]
        [AllowAnonymous]
        public HttpResponseMessage GetArtifact(string containerName, string blobName)
        {
            var blob = AzureStorageUtils.GetBlob(containerName, blobName);
            if (blob.Exists())
            {
                blob.FetchAttributes();
                var contentType = blob.Properties.ContentType;
                var ms = new System.IO.MemoryStream();
                blob.DownloadToStream(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(ms);
                // ctor MediaTypeHeaderValue() does not accept "text/plain; charset=utf-8" directly
                MediaTypeHeaderValue value;
                if (MediaTypeHeaderValue.TryParse(contentType, out value))
                {
                    result.Content.Headers.ContentType = value;
                }
                return result;
            }
            else
                return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        // GET: /api/exercises/cards/SVIS__/CDIS__
        [Route("cards/{serviceType}/{cardType}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetCards(string serviceType, string cardType)
        {
            var sql = @"
select Id, [Type], Title, CardId, Position, Contents
from dbo.exeCardsWithItems
where [Type] = @CardType;
";
            var cards = await ExerciseUtils.GetCardsWithItems(sql, new
            {
                CardType = CardType.GetCardType(serviceType, cardType)
            });
            return Ok(cards);
        }

        // GET: /api/exercises/user_card/SVRIS_
        [Route("user_card/{serviceType}")]
        public async Task<IHttpActionResult> GetUserCard(string serviceType)
        {
            var sql = @"
select C.Id, C.[Type], C.Title, C.CardId, C.Position, C.Contents
from dbo.exeUserCards UC
	inner join dbo.exeCardsWithItems C on UC.CardId = C.Id
where UC.UserId = @UserId
	and UC.ServiceType = @ServiceType;
";
            var card = (await ExerciseUtils.GetCardsWithItems(sql, new
            {
                UserId = this.GetUserId(),
                ServiceType = serviceType
            }))
            .FirstOrDefault();

            return Ok(card);
        }

        // PUT: /api/exercises/user_card/SVRIS_
        [Route("user_card/{type}")]
        public async Task<IHttpActionResult> PutUserCard([FromBody]JObject value, string type)
        {
            var sql = @"
merge dbo.exeUserCards as Trg
using (values(@UserId, @Type)) as Src (UserId, [Type])
	on Trg.UserId = Src.UserId and Trg.ServiceType = Src.[Type]
when matched then
	update set 
		CardId = @CardId
when not matched then
	insert (UserId, ServiceType, CardId)
		values (@UserId, @Type, @CardId);
";
            var rowsAffected = await DapperHelper.ExecuteResilientlyAsync(sql, new
            {
                UserId = this.GetUserId(),
                Type = type,
                CardId = (Guid)value["id"],
            });
            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
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

        // PUT /api/exercises/12345/length
        [Route("{id:int}/length")]
        public async Task<IHttpActionResult> PutLength(int id, [FromBody]JObject value)
        {
            await DapperHelper.ExecuteResilientlyAsync("dbo.exeUpdateLength",
                new
                {
                    ExerciseId = id,
                    UserId = this.GetUserId(),
                    Length = (int)value["length"],
                },
            CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/exercises/recording_details
        [Route("recording_details")]
        public async Task<IHttpActionResult> PutRecordingDetails([FromBody]JObject value)
        {
            // Execute the query even if the title is null, we need to send the ExerciseId back.
            var userId = this.GetUserId();
            var recorderId = (string)value["recorderId"];
            var recordName = (string)value["recordName"];
            var blobName = UploadUtils.ConstractArtifactBlobName(userId, recorderId, recordName);

            var title = UploadUtils.NormalizeExerciseTitle((string)value["title"]);
            var cardId = (Guid?)value["cardId"];
            var length = UploadUtils.DurationToLength((double)value["duration"]); // We use duration because there is a chance that the same instance of a recorder may be reused.

            // ServiceType corresponds to dbo.appServices
            var sql = @"
update dbo.exeExercises
set Title = @Title, CardId = @CardId
output deleted.Id
where UserId = @UserId 
    and [Length] = @Length 
    and Artifact = @Artifact;
";
            var exerciseId = (await DapperHelper.QueryResilientlyAsync<int>(sql,
                  new
                  {
                      Title = title,
                      CardId = cardId,
                      UserId = userId,
                      Length = length,
                      Artifact = blobName,
                  }))
                  .SingleOrDefault();

            return Ok(exerciseId);
        }

    }
}