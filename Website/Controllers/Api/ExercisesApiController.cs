using Dapper;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            // Returns 0 as Balance if the user has not created an account yet.
            var data = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new
            {
                UserId = this.GetUserId(),
                ExerciseId = id,
            }
            )).SingleOrDefault();
            return Ok(data);
        }

        // No need for async. This method is called only on the development machine. The production code downloads from the Blob Storage directly.
        // GET api/exercises/blob?containerName=writing-photos&blobName=0000000003/22273p29f8/2.jpg
        [AllowAnonymous]
        [Route("blob")]
        public HttpResponseMessage GetBlob(string container, string name)
        {
            var blob = AzureStorageUtils.GetBlob(container, name);
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
        [AllowAnonymous]
        [Route("cards/{serviceType}/{cardType}")]
        public async Task<IHttpActionResult> GetCards(string serviceType, string cardType)
        {
            var sql = @"
select Id, [Type], Title, CardId, Position, Content
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
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetUserCard(string serviceType)
        {
            var sql1 = @"
select CI.Id, CI.[Type], CI.Title, CI.CardId, CI.Position, CI.Content
from dbo.exeUserCards UC
	inner join dbo.exeCardsWithItems CI on UC.CardId = CI.Id
where UC.UserId = @UserId
	and UC.ServiceType = @ServiceType;
";
            var sql2 = @"
select Id, [Type], Title, CardId, Position, Content
from dbo.exeCardsWithItems
where Id = '4266FD13-32C6-412A-96EA-0B623FA82396';
";
            var userId = this.GetUserId();
            var card = (await ExerciseUtils.GetCardsWithItems(
                userId != 0 ? sql1 : sql2,
                new
                {
                    UserId = userId,
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

            var title = ExerciseUtils.NormalizeExerciseTitle((string)value["title"]);
            var cardId = (Guid?)value["cardId"];
            var length = UploadUtils.DurationToLength((double)value["duration"]); // We use duration because there is a chance that the same instance of a recorder may be reused.

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


        // POST /api/exercises/save_tracks?metadataName=metadata1234567890
        [AllowAnonymous]
        [Route("save_tracks")]
        public async Task<IHttpActionResult> PostTracks(string metadataName)
        {
            //~~ Validate.
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            // Parse the request body parts.
            var streamProvider = await Request.Content.ReadAsMultipartAsync();

            // Check mediaType of the files
            var mediaTypes = streamProvider.Contents
                .Select(i => i.Headers)
                .Where(i => i.ContentDisposition.Name.Trim('"') != metadataName)
                .Select(i => i.ContentType)
                //.Where(i => i != null) // ContentType is null in the 'metadata' part.
                .Select(i => i.MediaType)
                .Distinct()
                ;
            var mediaType = mediaTypes.FirstOrDefault();
            var acceptedMediaTypes = new[] { MediaType.Mpeg, MediaType.Mp3, MediaType.Amr, MediaType.Gpp, MediaType.QuickTime };
            if ((mediaTypes.Count() != 1) || !acceptedMediaTypes.Contains(mediaType))
            {
                return BadRequest(String.Join(", ", mediaTypes));
            }

            //~~ Save the original media files to blobs.

            var userId = this.GetUserId(); // May be 0 if the user is unauthenticated
            var userIdKey = KeyUtils.IntToKey(userId);
            var timeKey = KeyUtils.GetTimeAsBase32();
            // The length of extId is 12 chars.
            var extId = (userId != 0)
                ? null
                : this.GetExtId();
            var longTimeKey = timeKey + extId; // If an operand of string concatenation is null, an empty string is substituted.
            var extension = MediaType.GetExtension(mediaType);

            var tracks = new List<KeyValuePair<string, MemoryStream>>();
            var fileNames = new List<string>();

            foreach (var content in streamProvider.Contents)
            {
                var name = content.Headers.ContentDisposition.Name.Trim('"');
                var contentType = content.Headers.ContentType;
                // We have checked above, if ContentType has a value, it has the right MediaType.
                if ((name != metadataName) && (contentType != null))
                {
                    fileNames.Add(name);
                    var stream = await content.ReadAsStreamAsync();
                    using (stream)
                    {
                        var memStream = new MemoryStream();
                        await stream.CopyToAsync(memStream);
                        var pair = new KeyValuePair<string, MemoryStream>(name, memStream);
                        tracks.Add(pair);
                    }
                }
            }

            // Upload all blobs in parallel.
            var tasks = tracks.Select(i =>
            {
                var blobName = ExerciseUtils.FormatBlobName(userIdKey, longTimeKey, i.Key, extension);
                return AzureStorageUtils.UploadBlobAsync(i.Value, AzureStorageUtils.ContainerNames.Artifacts, blobName, mediaType);
            });
            await Task.WhenAll(tasks);

            //~~ Call the transcoding service.

            ///* We send the file names as a comma separated list. There is also a binding in the Web API like this:
            //public IHttpActionResult GetFoo([FromUri] int[] ids); Call: /Foo?ids=1&ids=2&ids=3 or /Foo?ids[0]=1&ids[1]=2&ids[2]=3
            //public IHttpActionResult GetFoo([FromUri] List<string> ids); Call: /Foo?ids[]="a"&ids[]="b"&ids[]="c"
            //*/
            //var host = ConfigurationManager.AppSettings["RecorderHost"];
            //var urlFormat = "http://{0}/api/recordings/transcoded/?userIdKey={1}&timeKey={2}&extension={3}&fileNames={4}";
            //var url = String.Format(urlFormat, host, userIdKey, longTimeKey, extension, String.Join(",", fileNames));
            //HttpClient client = new HttpClient();
            //HttpResponseMessage response = await client.GetAsync(url);
            //if (!response.IsSuccessStatusCode)
            //{
            //    // return RedirectToAction(referrerAction, new { error = "transcoding_error" });
            //    return InternalServerError(new Exception("Transcoding error. " + response.StatusCode.ToString()));
            //}
            //// Error is returned as HTML. Then we get error here: No MediaTypeFormatter is available to read an object of type 'JObject' from content with media type 'text/html'.
            //var recordingDetails = await response.Content.ReadAsAsync<RecordingDetails>();

            //// Make sure the duration is known. If the transcoder has failed to parse the ffmpeg logs, it returns DurationMsec = 0.
            //if (recordingDetails.TotalDuration == 0)
            //{
            //    // Read the blob and try to determine the duration directly.
            //    recordingDetails.TotalDuration =
            //        await RecordingUtils.GetMp3Duration(AzureStorageUtils.ContainerNames.Artifacts, recordingDetails.BlobName);
            //}

            var transcoder = new RecordingTranscoder();
            var recordingDetails = await transcoder.Transcode(tracks, userIdKey, longTimeKey, extension);

            // Release the memory streams.
            tracks.ForEach(i =>
            {
                i.Value.Dispose();
            });

            //~~ Read the metadata. 
            // Chrome wraps the part name in double-quotes.
            var metadataContent = streamProvider.Contents
                 .Single(i => i.Headers.ContentDisposition.Name.Trim('"') == metadataName)
                 ;
            var metadataJson = await metadataContent.ReadAsStringAsync();
            var metadata = JObject.Parse(metadataJson);
            var serviceType = (string)metadata["serviceType"];
            var cardId = (Guid?)metadata["cardId"];
            var title = (string)metadata["title"];
            var comment = (string)metadata["comment"];

            var serializer = new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var details = JObject.FromObject(recordingDetails, serializer);

            if (userId != 0)
            {
                //~~ Create a database record.
                var exerciseId = await ExerciseUtils.CreateExercise(recordingDetails.BlobName, userId,
                    serviceType, ArtifactType.Mp3, recordingDetails.TotalDuration, title, cardId, comment, details.ToString(Formatting.None));

                //~~ The client will redirect to the View exercise page.
                return Ok(new { ExerciseId = exerciseId });
            }
            else
            {
                //~~ Save the details for future use.
                metadata.Add(new JProperty("recordingDetails", details));
                var blobName = ExerciseUtils.FormatBlobName(userIdKey, longTimeKey, "metadata", "json");
                await AzureStorageUtils.UploadTextAsync(metadata.ToString(), AzureStorageUtils.ContainerNames.Artifacts, blobName, MediaType.Json);
                //~~ The client will redirect to the Signup page. longTimeKey will be built from timeKey on the Claim page.
                return Ok(new { Key = timeKey });
            }
        }

    }
}