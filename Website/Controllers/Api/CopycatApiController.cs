using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runnymede.Common.LibraryIndex;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [RoutePrefix("api/copycat")]
    public class CopycatApiController : ApiController
    {

        // GET: /api/copycat/resources?offset=0&limit=10&viewed=false&session=123
        [Route("resources")]
        public async Task<IHttpActionResult> GetResources(int offset, int limit, bool viewed, int session)
        {
            var result = new DapperHelper.PageItems<object>();

            if (this.IsAuthenticated() || !viewed)
            {


                result = await DapperHelper.QueryPageItems<dynamic>("dbo.copGetResources",
                    new
                    {
                        UserId = this.GetUserId(),
                        RowOffset = offset,
                        RowLimit = limit,
                        Viewed = viewed,
                        Session = session,
                    }
                    );
            }

            return Ok(result);
        }

        // GET: /api/copycat/transcript/200000001
        [Route("transcript/{id:int}")]
        public async Task<IHttpActionResult> GetTranscript(int id)
        {
            var partitionKey = KeyUtils.IntToKey(id);
            var filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            // Since we do RowKey = KeyUtils.DateTimeToDescKey(now), the last written record is at the top.
            var query = new TableQuery<GameCopycatEntity2>().Where(filterPartition).Take(1);
            var entities = await AzureStorageUtils.ExecuteQueryAsync(AzureStorageUtils.TableNames.GameCopycat, query);
            var result = entities.Any() ? entities.Single() : new object();
            return Ok(result);
        }

        // POST: /api/copycat/validate
        [Route("validate")]
        public async Task<IHttpActionResult> PostValidate([FromBody] JObject value)
        {
            var url = (string)value["url"];
            var resource = await LibraryUtils.TryValidateResource(url);
            return Ok(resource);
        }

        // POST: /api/copycat/resource/
        [Route("resource")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PostResource([FromBody] JObject value)
        {
            var start = (int?)value["start"];
            var finish = (int?)value["finish"];
            var hasSegment = start.HasValue && finish.HasValue;
            if (hasSegment && ((start.Value < 0) || (finish.Value < 0) || (start >= finish)))
            {
                return BadRequest("Segment time is wrong.");
            }

            var url = (string)value["url"];
            var resource = await LibraryUtils.TryValidateResource(url);

            // If validation is unsuccessfull, TryValidateResource returns null
            if ((resource == null) || (resource.Format != LibraryUtils.Formats.Youtube))
            {
                return BadRequest("Resource not found.");
            }

            // Idempotent. The stored procedure returns the processed resource back.
            resource = (await DapperHelper.QueryResilientlyAsync<ResourceDto>("dbo.libCreatePersonalResource",
                   new
                   {
                       UserId = this.GetUserId(),
                       Format = resource.Format,
                       NaturalKey = resource.NaturalKey,
                       Segment = hasSegment ? Resource.BuildSegment(start.Value, finish.Value) : null,
                       Title = resource.Title,
                       CategoryIds = LibraryUtils.KnownCategories.GameCopycat,
                       Tags = LibraryUtils.KnownTags.GameCopycat,
                       HasVideo = resource.HasVideo,
                       IsForCopycat = true,
                   },
                   CommandType.StoredProcedure))
                   .Single();

            var transcript = (string)value["transcript"];
            if (!String.IsNullOrEmpty(transcript))
            {
                await InternalSaveTranscript(resource.Id, transcript);
            }

            return Ok(resource);
        }

        // POST: /api/copycat/transcript/200000001
        [Route("transcript/{id:int}")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PostTranscript([FromUri] int id, [FromBody] JObject value)
        {
            var transcript = (string)value["transcript"];
            if (String.IsNullOrEmpty(transcript))
            {
                return BadRequest("Transcript is empty.");
            }
            await InternalSaveTranscript(id, transcript);
            return StatusCode(HttpStatusCode.Created);
        }

        // PUT: /api/copycat/200000001/priority
        [Route("{id:int}/priority")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PutPriority(int id, [FromBody] JObject value)
        {
            await DapperHelper.ExecuteResilientlyAsync("dbo.copUpdatePriority",
                 new
                 {
                     UserId = this.GetUserId(),
                     ResourceId = id,
                     Priority = (int)value["priority"],
                 },
                 CommandType.StoredProcedure
                 );

            return StatusCode(HttpStatusCode.NoContent);
        }

        private async Task InternalSaveTranscript(int resourceId, string transcript)
        {
            var entity = new GameCopycatEntity2
            {
                PartitionKey = KeyUtils.IntToKey(resourceId),
                RowKey = KeyUtils.DateTimeToDescKey(DateTime.UtcNow),
                Transcript = transcript,
                UserId = this.GetUserId(),
                UserDisplayName = this.GetUserDisplayName(),
            };
            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.GameCopycat, entity);
        }

    }
}
