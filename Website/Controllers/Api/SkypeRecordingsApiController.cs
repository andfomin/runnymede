using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/skype_recordings")]
    public class SkypeRecordingsApiController : ApiController
    {
        private const int AssumedTeacherReactionDelayMSec = 0; // We might allow for a delay between the actual learner's mistake and the teacher's action.
        private const int DefaultSpotLengthMSec = 3000; // Initially produce a 3 seconds long spot.

        // PUT api/skype_recordings/
        [Route("")]
        public async Task<IHttpActionResult> PutStart([FromBody] JObject values)
        {
            var learnerSkype = (string)values["learnerSkype"];

            // Make sure the teacher's Skype Name is known. We use it to filter the ongoing calls.
            var sqlTeacher = @"
select SkypeName from dbo.appGetUser(@UserId);
";
            var skypeName = (await DapperHelper.QueryResilientlyAsync<string>(sqlTeacher, new { UserId = this.GetUserId(), }))
                .SingleOrDefault();

            if (String.IsNullOrEmpty(skypeName))
            {
                throw new Exception("Please enter your Skype name on the Profile page.");
            }

            // Make sure the learner's Skype Name is known
            var sqlLearner = @"
select cast(coalesce(dbo.appGetUserIdBySkypeName(@SkypeName), 0) as bit);
";
            var learnerFound = (await DapperHelper.QueryResilientlyAsync<bool>(sqlLearner, new { SkypeName = learnerSkype, }))
                .SingleOrDefault();

            if (!learnerFound)
            {
                throw new Exception("The Skype name was not found. Ask the learner to enter their Skype name on the Profile page.");
            }

            var helper = new FreeSwitchHelper();

            //-- Find the Skype call. Get the Uuid of the Skype call assigned by Freeswitch.
            var path = "txtapi/show?calls";
            var response = await helper.SendRpc(path);

            string uuid = null;
            //DateTime? created = null;
            var lines = response.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() > 1)
            {
                var callValues = lines.ElementAt(1).Split(new[] { ',' });
                uuid = callValues[0];
                //var createdValue = DateTime.ParseExact(values[2], "yyyy-MM-dd HH:mm:ss", null);
                // Freeswitch reports the server's local time and it does not report the timezone. We expect the VM's time is UTC (This is the case on Azure.)
                //DateTime.SpecifyKind(createdValue, DateTimeKind.Utc);
                //created = createdValue;
            }
            Guid guid; // uuid is a string which represents a Guid.
            if (!Guid.TryParse(uuid, out guid))
            {
                throw new HttpException("Skype call not found.");
            }

            //-- Play announce
            //path = String.Format("txtapi/uuid_broadcast?{0}%20misc/call_monitoring_blurb.wav", uuid);
            //await helper.SendRpc(path, "+OK Message sent");
            path = String.Format("txtapi/uuid_broadcast?{0}%20ivr/ivr-recording_started.wav", uuid);
            await helper.SendRpc(path, "+OK Message sent");
            // Freeswitch returns 200 OK immidiately, even if the sound will then play longer. Without a delay the announcement will be heard on the recording.
            await Task.Delay(2000); // 3 sec + 1 sec + 1 sec extra

            //-- Start recording
            var sourceDir = ConfigurationManager.AppSettings["Freeswitch.MyRecordingsDir"];
            //  Freeswitch expects a path with forward slash separators. Windows tolerates them as well. We escape the '\' as '\\'.
            sourceDir = sourceDir.Replace('\\', '/');
            var limit = 900; // 15 minutes * 60 = 900 seconds. Limit the maximum recording time.
            path = String.Format("webapi/uuid_record?{0} start '{1}/{0}.wav' {2}", uuid, sourceDir, limit);
            await helper.SendRpc(path, "+OK Success");

            //-- Inject the tone mark at the beginning of the recording. We might use it to correlate the remark spots relativaly to the start.
            // tone_stream://L=1;v=0;%(200,0,1000)
            path = String.Format("txtapi/uuid_broadcast?{0} tone_stream%3A%2F%2FL=1;v=0;%25(200,0,1000)", uuid);
            await helper.SendRpc(path, "+OK Message sent");

            return Created(uuid, uuid);
        }

        // By the way, the "mask" and "unmask" commands do not work, even in fs_cli.
        // POST api/skype_recordings/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/stop
        [Route("{uuid}/stop")]
        public async Task<IHttpActionResult> PostStop(string uuid)
        {
            //-- Stop recording.
            var path = String.Format("webapi/uuid_record?{0} stop all", uuid);
            var helper = new FreeSwitchHelper();
            await helper.SendRpc(path, "+OK Success");

            //-- Inform the sides.
            path = String.Format("txtapi/uuid_broadcast?{0}%20ivr/ivr-recording_stopped.wav", uuid);
            await helper.SendRpc(path, "+OK Message sent");
            await Task.Delay(2000); // 1 sec + 1 sec extra

            //-- Hang up the call from the Freeswitch side.
            path = String.Format("webapi/uuid_kill?{0}", uuid);
            await helper.SendRpc(path, "+OK");

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/skype_recordings/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/save
        [Route("{uuid}/save")]
        public async Task<IHttpActionResult> PutSave([FromUri] string uuid, [FromBody] JObject values)
        {
            string redirectToUrl = null;

            var learnerSkype = (string)values["learnerSkype"];
            var titleValue = (string)values["title"];
            var comment = (string)values["comment"];
            var remarkSpots = ((JArray)values["remarkSpots"]).Select(i => (int)i); // milliseconds

            var userId = this.GetUserId();

            //-- Call the remote transcoding service.
            var host = ConfigurationManager.AppSettings["RecorderHost"];
            var urlFormat = "http://{0}/api/recordings/from_freeswitch/?uuid={1}&userId={2}";
            var url = String.Format(urlFormat, host, uuid, userId);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException("from_freeswitch");
            }
            // Error is returned as HTML. Then we get error here: No MediaTypeFormatter is available to read an object of type 'RecordingInfo' from content with media type 'text/html'.
            var recordingDetails = await response.Content.ReadAsAsync<RecordingDetails>();
            // Make sure the duration is known. If the transcoder has failed to parse the ffmpeg logs, it returns DurationMsec = 0.
            if (recordingDetails.TotalDuration == 0)
            {
                // Read the blob and try to determine the duration directly.
                recordingDetails.TotalDuration = 
                    await RecordingUtils.GetMp3Duration(AzureStorageUtils.ContainerNames.Artifacts, recordingDetails.BlobName);
            }

            var title = !String.IsNullOrEmpty(titleValue)
                ? titleValue
                : Path.GetFileNameWithoutExtension(recordingDetails.BlobName);

            //-- Create a database record for the exercise.
            var exerciseId = await ExerciseUtils.CreateExercise(recordingDetails.BlobName, userId,
                ServiceType.IeltsSpeaking, ArtifactType.Mp3, recordingDetails.TotalDuration, title, null, comment);

            // If the recording is being uploaded by a teacher, make it owned by the learner, create a review, and save remark spots if an accompanying XML file is provided.
            if (exerciseId != 0 && this.GetUserIsTeacher())
            {
                //-- Change the exercise author.
                // We may succeed or fail with updating the user of the exercise depending on whether the provided Skype name is found and is unambiguous.
                // Continue anyway with either old or new user.
                var newUserId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.exeTryChangeExerciseAuthor",
                                          new
                                          {
                                              ExerciseId = exerciseId,
                                              UserId = userId,
                                              SkypeName = learnerSkype,
                                          },
                                          CommandType.StoredProcedure))
                                          .SingleOrDefault();

                //-- Create a review. We do not charge the learner for a review initiated by the teacher.
                var reviewId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.exeCreateUploadedReview",
                    new
                    {
                        ExerciseId = exerciseId,
                        UserId = userId,
                    },
                    CommandType.StoredProcedure))
                    .SingleOrDefault();

                //-- We have got remark spots. Save remarks.
                if (reviewId != 0 && remarkSpots.Any())
                {
                    var pieces = remarkSpots
                        .OrderBy(i => i)
                        .Distinct()
                        .Select((i, index) =>
                        {
                            var finish = Math.Max(0, i - AssumedTeacherReactionDelayMSec);
                            var start = Math.Max(0, finish - DefaultSpotLengthMSec); 

                            var remark = new RemarkSpot
                            {
                                ReviewId = reviewId,
                                Type = ReviewPiece.PieceTypes.Remark,
                                Id = index,
                                Start = start,
                                Finish = finish,
                            };

                            return new ReviewPiece
                            {
                                // This PartitionKey calculation is redundunt, but the overhead is neglectable.
                                PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                                RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Remark, index),
                                Json = JsonUtils.SerializeAsJson(remark),
                            };
                        });

                    var batchOperation = new TableBatchOperation();
                    foreach (var piece in pieces)
                    {
                        batchOperation.InsertOrReplace(piece);
                    }

                    //-- Write entities which will allow the reviewer to access remarks for reading and writing. We will simply check the presence of one of these records as we read or write the entities.
                    // The write entity will be deleted on review finish.
                    var viewerEntity = new ReviewPiece()
                    {
                        PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                        RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Viewer, userId),
                    };
                    batchOperation.InsertOrReplace(viewerEntity);

                    var editorEntity = new ReviewPiece()
                    {
                        PartitionKey = ReviewPiece.GetPartitionKey(exerciseId),
                        RowKey = ReviewPiece.GetRowKey(reviewId, ReviewPiece.PieceTypes.Editor, userId),
                    };
                    batchOperation.InsertOrReplace(editorEntity);

                    var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.ReviewPieces);
                    await table.ExecuteBatchAsync(batchOperation);

                } // end of if (reviewId != 0 && remarkSpots.Any())

                //-- Return the link to the review page. The client will redirect.
                redirectToUrl = Url.Link("Default", new
                {
                    Controller = "Reviews",
                    Action = reviewId != 0 ? "Edit" : "Index",
                    id = reviewId.ToString()
                });
            } // end of if (exerciseId && this.GetIsTeacher())

            return Created(redirectToUrl, redirectToUrl);
        }



    }
}
