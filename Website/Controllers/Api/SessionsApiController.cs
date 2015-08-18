using Dapper;
using Itenso.TimePeriod;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Linq;
using Twilio;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/sessions")]
    public class SessionsApiController : ApiController
    {

        // GET api/sessions/MillisSinceEpoch
        [Route("millis_since_epoch")]
        [AllowAnonymous]
        public Int64 GetMillisSinceEpoch()
        {
            // Calculate the difference on the client between the local time and provided time. It is used to show a warning on the page if the local clock is wrong.
            // JavaScript uses milliseconds since the Unix epoch. 
            return TimeUtils.GetMillisecondsSinceEpoch(DateTime.UtcNow);
        }

        // GET api/sessions/schedule
        [Route("schedule")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetSchedule(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            SessionHelper.ConvertFullCalendarDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);

            var isTeacher = this.GetUserIsTeacher();
            // Get the sessions related to the user
            var sessions = (await DapperHelper.QueryResilientlyAsync<SessionDto>(
                isTeacher ? "dbo.sesGetSessionsForTeacher" : "dbo.sesGetSessionsForLearner",
                new
                {
                    UserId = this.GetUserId(),
                    Start = startDate,
                    End = endDate,
                },
            CommandType.StoredProcedure))
            .ToList();

            if (!isTeacher)
            {
                var offeredSessions = await SessionHelper.GetOfferedSchedules(startDate, endDate, false);
                // Merge offers with sessions.
                sessions.AddRange(offeredSessions);
            }

            return Ok(sessions);
        }

        // GET api/sessions/offers
        [Route("offers")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetSessionOffers(string start, string end, string localTime, int localTimezoneOffset)
        {
            object result = null;

            TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

            DateTime startDate, endDate;
            if ((!DateTime.TryParse(start, null, DateTimeStyles.RoundtripKind, out startDate))
                ||
                (!DateTime.TryParse(end, null, DateTimeStyles.RoundtripKind, out endDate)))
            {
                throw new ArgumentException("Date is wrong.");
            }

            var sessions = await SessionHelper.GetOfferedSessions(startDate, endDate);

            if (sessions.Any())
            {
                // Query the display names from the database. dbo.appUsers has no SELECT permision. We work around it by using the existing function.
                //var sqlParts = sessions
                //    .Select(i => i.TeacherUserId)
                //    .Distinct()
                //    .Select(i => String.Format(@"select Id, DisplayName from dbo.appGetUser({0})", i));
                //var sql = String.Join(" union ", sqlParts);
                //IEnumerable<dynamic> displayNames = await DapperHelper.QueryResilientlyAsync<dynamic>(sql);

                result = sessions
                    .OrderBy(i => i.Start)
                    .ThenBy(i => i.TeacherUserId)
                    .Select(i =>
                    {
                        var teacher = SessionHelper.ItalkiTeachers.First(j => j.UserId == i.TeacherUserId);
                        return new
                        {
                            Start = i.Start,
                            End = i.End,
                            TeacherUserId = i.TeacherUserId,
                            Price = teacher.Rate * (decimal)(i.End - i.Start).TotalHours,
                            DisplayName = teacher.DisplayName,
                        };
                    });
            }

            return Ok(result);
        }

        // GET /api/sessions/conditions
        [Route("conditions")]
        public async Task<IHttpActionResult> GetConditions()
        {
            var data = await GetSessionConditions(this.GetUserId());
            return Ok(data);
        }

        // GET api/sessions/12345/other_user
        [Route("{id:int}/other_user")]
        public async Task<IHttpActionResult> GetOtherUser(int id)
        {
            var item = (await DapperHelper.QueryResilientlyAsync<dynamic>("dbo.sesGetOtherUser", new
                   {
                       SessionId = id,
                       UserId = this.GetUserId(),
                   },
                   CommandType.StoredProcedure))
                // The query may return empty rowset. We need a typed data to return Ok<T>(T). Otherwise it cannot infer the type parameter when given a null.
                   .Select(i => new
                   {
                       Id = (int)i.Id,
                       DisplayName = (string)i.DisplayName,
                       SkypeName = (string)i.SkypeName,
                   })
                   .SingleOrDefault();

            return Ok(item);
        }

        // GET api/sessions/12345/message_count
        [Route("{id:int}/message_count")]
        public async Task<IHttpActionResult> GetMessageCount(int id)
        {
            const string sql = @"
select dbo.sesGetMessageCount(@UserId, @SessionId);
";
            int messageCount;
            // This query is executed in pooling. We do not need resiliency here. So avoid overhead, don't use DapperHelper.QueryResilientlyAsync
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                messageCount = (await connection.QueryAsync<int>(sql,
                    new
                    {
                        UserId = this.GetUserId(),
                        SessionId = id,
                    }))
                    .Single();
            }

            return Ok(messageCount);
        }

        // GET api/sessions/12345/messages
        [Route("{id:int}/messages")]
        public async Task<IHttpActionResult> GetMessages(int id)
        {
            var items = await DapperHelper.QueryResilientlyAsync<dynamic>("dbo.sesGetMessages", new
            {
                UserId = this.GetUserId(),
                SessionId = id,
            },
            CommandType.StoredProcedure
            );

            return Ok(items);
        }

        // GET api/sessions/message_text/234567qwerty
        [Route("message_text/{extId:length(12)?}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetMessageText(string extId = null)
        {
            string text = null;
            if (!String.IsNullOrWhiteSpace(extId))
            {
                var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.UserMessages);
                var operation = TableOperation.Retrieve<UserMessageEntity>(extId, String.Empty);
                var result = await table.ExecuteAsync(operation);
                var entity = result.Result as UserMessageEntity;
                text = entity != null ? entity.Text : null;
            }
            return new RawStringResult(this, text, RawStringResult.TextMediaType.PlainText);
        }

        // POST /api/sessions/booking
        [Route("booking")]
        public async Task<IHttpActionResult> PostBooking([FromBody] JObject json)
        {
            TimeUtils.ThrowIfClientTimeIsAmbiguous((string)json["localTime"], (int)json["localTimezoneOffset"]);

            var teacherUserId = (int)json["teacherUserId"];
            var start = (DateTime)json["start"];
            var end = (DateTime)json["end"];
            var price = (decimal)json["price"];

            var userId = this.GetUserId();

            var teacher = SessionHelper.ItalkiTeachers.First(i => i.UserId == teacherUserId);

            var periods = await SessionHelper.GetTeacherPeriods(teacherUserId, true);

            var available = periods.Any(i => (i.Start <= start) && (i.End >= end));

            if (!available)
            {
                throw new Exception(ItalkiHelper.TimeSlotUnavailableError);
            }

            /* Compensation is the mechanism by which previously completed work can be undone or compensated when a subsequent failure occurs. 
             * The Compensation pattern is required in error situations when multiple atomic operations cannot be linked with classic transactions. 
             */

            // We cannot use a transaction in a heterogeneous storage mediums. An orphaned message on Azure Table is not a problem, we do not compensate it on error. The main message metadata is in the database anyway. 
            var message = (string)json["message"];
            var messageExtId = await SessionHelper.SaveMessageAndGetExtId(message);

            var sessionId = (await DapperHelper.QueryResilientlyAsync<int>("dbo.sesBookSession", new
             {
                 UserId = userId, // who requests and pays
                 Start = start,
                 End = end,
                 TeacherUserId = teacherUserId,
                 Price = price,
                 MessageExtId = messageExtId,
             },
             CommandType.StoredProcedure))
             .Single();

            // We will write to the log.
            var partitionKey = KeyUtils.GetCurrentTimeKey();

            // Book the session on Italki.
            try
            {
                long? sessionExtId = null;
#if DEBUG
                sessionExtId = 0;
#else
                var helper = new ItalkiHelper();
                sessionExtId = await helper.BookSession(teacher.CourseId, start, teacher.ScheduleUrl, partitionKey);
#endif
                if (sessionExtId.HasValue)
                {
                    await DapperHelper.ExecuteResilientlyAsync("dbo.sesUpdateSessionExtId", new
                    {
                        SessionId = sessionId,
                        ExtId = sessionExtId,
                    },
                    CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                // Compensation on booking failure.
                // Clean up the session.
                DapperHelper.ExecuteResiliently("dbo.sesCancelSession", new
                {
                    UserId = userId,
                    SessionId = sessionId,
                    ClearUsers = true,
                },
                CommandType.StoredProcedure);

                throw new UserAlertException("Failed to book session", ex);
            }

            HostingEnvironment.QueueBackgroundWorkItem(async (ct) => await SendSmsMessage("Session request. Teacher " + teacherUserId.ToString(), partitionKey));

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/sessions/1234567890/confirmation
        [Route("{id:int}/confirmation")]
        public async Task<IHttpActionResult> PostConfirmation(int id)
        {
            var confirmationTime = (await DapperHelper.QueryResilientlyAsync<DateTime>("dbo.sesConfirmSession", new
            {
                UserId = this.GetUserId(),
                SessionId = id,
            },
            CommandType.StoredProcedure))
            .Single();

            return Ok(new { ConfirmationTime = confirmationTime });
        }

        // POST /api/sessions/1234567890/cancellation
        [Route("{id:int}/cancellation")]
        public async Task<IHttpActionResult> PostCancellation(int id)
        {
            var cancellationTime = (await DapperHelper.QueryResilientlyAsync<DateTime>("dbo.sesCancelSession", new
            {
                UserId = this.GetUserId(),
                SessionId = id,
            },
            CommandType.StoredProcedure))
            .Single();

            return Ok(new { CancellationTime = cancellationTime });
        }

        // POST /api/sessions/12345/message
        [Route("{id:int}/message")]
        public async Task<IHttpActionResult> PostMessage(int id, [FromBody] JObject json)
        {
            var message = (string)json["message"];
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            var messageExtId = await SessionHelper.SaveMessageAndGetExtId(message);

            await DapperHelper.ExecuteResilientlyAsync("dbo.sesPostMessage",
                new
                {
                    UserId = this.GetUserId(),
                    SessionId = id,
                    MessageExtId = messageExtId,
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/sessions/accepted_proposals
        [Route("accepted_proposals")]
        public async Task<IHttpActionResult> PostAcceptedProposals([FromBody] IEnumerable<SessionDto> offers)
        {
            var proposalsXml =
                new XElement("Proposals",
                    from offer in offers
                    select new XElement("Proposal", new XAttribute("Id", offer.Id), new XAttribute("Cost", offer.Cost))
                    )
                    .ToString(SaveOptions.DisableFormatting);

            await DapperHelper.ExecuteResilientlyAsync("dbo.sesAcceptProposals",
                new
                {
                    UserId = this.GetUserId(),
                    Proposals = proposalsXml,
                },
                CommandType.StoredProcedure
                );

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/sessions/message_read/12345/234567abcdef
        [Route("message_read/{id:int}/{extId:length(12)?}")]
        public async Task<IHttpActionResult> PutMessageRead([FromUri] int id, [FromUri] string extId = null)
        {
            await DapperHelper.ExecuteResilientlyAsync("dbo.sesUpdateMessageRead",
                new
                {
                    UserId = this.GetUserId(),
                    MessageId = id,
                    MessageExtId = extId,
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/sessions/12345/rating
        [Route("{id:int}/rating")]
        public async Task<IHttpActionResult> PutRating([FromUri] int id, [FromBody] JObject json)
        {
            // Idempotent
            await DapperHelper.ExecuteResilientlyAsync("dbo.sesUpdateRating",
                new
                {
                    UserId = this.GetUserId(),
                    SessionId = id,
                    Rating = (byte)json["rating"],
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // Mandrill does POST with Content-Type: application/x-www-form-urlencoded
        public class InboundWebhookFormModel
        {
            public string mandrill_events { get; set; }
        }

        /// Notify about an email from italki. The email originally goes to GMail, forwarded by a filter to Mandrill, which calls this webhook.
        // POST /api/sessions/inbound_webhook   // Content-Type:application/x-www-form-urlencoded
        [AllowAnonymous]
        [AcceptVerbs("POST", "HEAD")] // Mandrill uses a HEAD request to validate the webhook address during its creation.
        [Route("inbound_webhook")]
        public async Task<IHttpActionResult> InboundWebhook(InboundWebhookFormModel form)
        {
            // The form is empty on a HEAD request
            if (form != null)
            {
                var mandrill_events = form.mandrill_events;
                var partitionKey = KeyUtils.GetCurrentTimeKey();

                var entity = new ExternalSessionLogEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = "InboundWebhook",
                    Data = mandrill_events,
                };
                await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.ExternalSessions, entity);

                await SendSmsMessage("InboundWebhook " + partitionKey, partitionKey);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        private async Task SendSmsMessage(string text, string tablePartitionKey)
        {
            // Send SMS
            var accountSid = ConfigurationManager.AppSettings["Twilio.AccountSid"];
            var authToken = ConfigurationManager.AppSettings["Twilio.AuthToken"];
            var fromPhoneNumber = ConfigurationManager.AppSettings["Twilio.PhoneNumber.SMS"];
            var twilio = new TwilioRestClient(accountSid, authToken);
            var message = twilio.SendMessage(fromPhoneNumber, "+16477711715", text);

            var entity = new ExternalSessionLogEntity
             {
                 PartitionKey = tablePartitionKey,
                 RowKey = "TwilioMessage",
                 Data = JsonUtils.SerializeAsJson(new { Message = message, }),
             };
            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.ExternalSessions, entity);
        }

        private async Task<dynamic> GetSessionConditions(int userId)
        {
            const string sql = @"
select dbo.accGetBalance(@UserId) as Balance, dbo.appGetServicePrice(@ServiceType) as Price
";
            // Returns null for Balance the user has not created the account yet.
            var data = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new
            {
                UserId = userId,
                ServiceType = ServiceType.Session,
            }
            )).Single();

            return data;
        }

    }
}