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

        // GET /api/exercises/review_conditions?exerciseType=EX____&length=10000
        [Route("review_conditions")]
        public async Task<IHttpActionResult> GetReviewConditions(string exerciseType, int length)
        {
            var sql = @"
select cast(1 as decimal(9,2)); -- dbo.exeCalculateReviewPrice(@ExerciseType, @Length);
select dbo.accGetReviewBalance(@UserId);
";
            object result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                sql,
                new
                {
                    UserId = this.GetUserId(),
                    ExerciseType = exerciseType,
                    Length = length,
                },
                CommandType.Text,
                (Dapper.SqlMapper.GridReader reader) =>
                {
                    result = new
                      {
                          Price = reader.Read<decimal>().Single(),
                          Balance = reader.Read<decimal?>().Single(),
                      };
                });
            return Ok(result);
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

        // GET: /api/exercises/cards/ielts/writing/2a
        [Route("cards/{exam}/{test}/{part}")]
        public async Task<IHttpActionResult> GetCards(string exam, string test, string part)
        {
            var sql = @"
select C.Id, C.[Type], C.Title, CI.CardId, CI.Id, CI.Position, CI.Contents
from dbo.exeCards C
	inner join dbo.exeCardItems CI on C.Id = CI.CardId
where C.[Type] = @Type;
";
            var cards = await InternalGetCards(sql, new { Type = MapCardType(exam, test, part) });
            return Ok(cards);
        }

        // GET: /api/exercises/user_card/ielts/writing/2a
        [Route("user_card/{exam}/{test}/{part}")]
        public async Task<IHttpActionResult> GetCard(string exam, string test, string part)
        {
            string type = MapCardType(exam, test, part);
            var sql = @"
select C.Id, C.[Type], C.Title, CI.CardId, CI.Id, CI.Position, CI.Contents
from dbo.exeUserCards U
	inner join dbo.exeCards C on U.CardId = C.Id
	inner join dbo.exeCardItems CI on C.Id = CI.CardId
where U.UserId = @UserId
	and C.[Type] = @Type;
";
            var card = (await InternalGetCards(sql, new
            {
                UserId = this.GetUserId(),
                Type = MapCardType(exam, test, part)
            }))
            .FirstOrDefault();

            return Ok(card);
        }

        private async Task<IEnumerable<CardDto>> InternalGetCards(string sql, object param)
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
                });

            return cards;
        }

        // PUT: /api/exercises/user_card/ielts/writing/2a
        [Route("user_card/{exam}/{test}/{part}/{id:int}")]
        public async Task<IHttpActionResult> PutUserCard(string exam, string test, string part, int id)
        {
            var sql = @"
merge dbo.exeUserCards as Trg
using (values(@UserId, @Type)) as Src (UserId, [Type])
	on Trg.UserId = Src.UserId and Trg.[Type] = Src.[Type]
when matched then
	update set 
		CardId = @CardId
when not matched then
	insert (UserId, [Type], CardId)
		values (@UserId, @Type, @CardId);
";
            var rowsAffected = await DapperHelper.ExecuteResilientlyAsync(sql, new
            {
                UserId = this.GetUserId(),
                Type = MapCardType(exam, test, part),
                CardId = id,
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

        private string MapCardType(string exam, string test, string part)
        {
            // Type corresponds to dbo.appTypes
            var path = exam + test + part;
            string type = null;
            switch (path)
            {
                case "ieltswriting1a":
                    type = "TSIW1A";
                    break;
                case "ieltswriting1g":
                    type = "TSIW1G";
                    break;
                case "ieltswriting2a":
                    type = "TSIW2A";
                    break;
                case "ieltswriting2g":
                    type = "TSIW2G";
                    break;
                case "ieltsspeaking1":
                    type = "TSIS1_";
                    break;
                case "ieltsspeaking23":
                    type = "TSIS23";
                    break;
                default:
                    throw new ArgumentException(path);
            }
            return type;
        }

    }
}