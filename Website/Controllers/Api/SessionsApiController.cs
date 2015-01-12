using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Dapper;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Common.Utils;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/sessions")]
    public class SessionsApiController : ApiController
    {

        // DELETE /api/sessions/vacant_time
        [Route("vacant_time")]
        public async Task<IHttpActionResult> DeleteVacantTime(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            ConvertDateTimePickerDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);

            // This operation is idempotent.
            await DapperHelper.ExecuteResilientlyAsync("dbo.sesDeleteVacantTime",
                new
                {
                    UserId = this.GetUserId(),
                    Start = startDate,
                    End = endDate,
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET api/sessions/MillisSinceEpoch
        [Route("millis_since_epoch")]
        [AllowAnonymous]
        public Int64 GetMillisSinceEpoch()
        {
            // Calculate the difference on the client between the local time and provided time. It is used to show a warning on the page if the local clock is wrong.
            // JavaScript uses milliseconds since the Unix epoch. 
            return TimeUtils.GetMillisecondsSinceEpoch(DateTime.UtcNow);
        }

        // GET api/sessions/own_schedule
        [Route("own_schedule")]
        public async Task<IHttpActionResult> GetOwnSchedule(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            ConvertFullCalendarDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);

            var sql = @"
select Id, Start, [End], [Type], @UserId as UserId
from dbo.sesScheduleEvents
where UserId = @UserId
    and Start <= @End
    and [End] >= @Start;
";
            var events = await DapperHelper.QueryResilientlyAsync<ScheduleEventDto>(sql, new
            {
                UserId = this.GetUserId(),
                Start = start,
                End = end,
            });

            return Ok(events);
        }

        // GET api/sessions/friend_schedules
        [Route("friend_schedules")]
        public async Task<IHttpActionResult> GetFriendSchedules(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            ConvertFullCalendarDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);

            IEnumerable<SessionUserDto> result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                  "dbo.sesGetFriendSchedules",
                  new
                  {
                      Start = startDate,
                      End = endDate,
                      UserId = this.GetUserId(),
                  },
                  CommandType.StoredProcedure,
                  (Dapper.SqlMapper.GridReader reader) =>
                  {
                      result = reader.Map<SessionUserDto, ScheduleEventDto, int>(u => u.Id, i => i.UserId, (u, i) => { u.ScheduleEvents = i; });
                  });

            return Ok(result);
        }

        // GET api/sessions/Offers
        [Route("offers")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetOffers(string date, int startHour, int endHour, string localTime, int localTimezoneOffset)
        {
            // The Date and Hours values come from the custom filter. Date comes as local midnight expressed for the UTC timezone.
            TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

            DateTime dateValue;
            if (!DateTime.TryParse(date, null, DateTimeStyles.RoundtripKind, out dateValue))
            {
                return BadRequest("Date is wrong.");
            }

            // We allow for an anonymous user.
            var tryUserId = this.GetUserId();
            var userId = tryUserId != 0 ? tryUserId : (int?)null;

            IEnumerable<SessionUserDto> result = null;
            await DapperHelper.QueryMultipleResilientlyAsync(
                  "dbo.sesGetOffers",
                  new
                  {
                      Start = dateValue.AddHours(startHour),
                      End = dateValue.AddHours(endHour),
                      UserId = userId,
                  },
                  CommandType.StoredProcedure,
                  (Dapper.SqlMapper.GridReader reader) =>
                  {
                      result = reader.Map<SessionUserDto, ScheduleEventDto, int>(u => u.Id, i => i.UserId, (u, i) => { u.ScheduleEvents = i; });
                  });

            return Ok(result);
        }

        // GET api/sessions/?eventId=12345
        [Route("")]
        public async Task<IHttpActionResult> GetSession(int eventId)
        {
            dynamic session = null;
            dynamic messages = null;

            await DapperHelper.QueryMultipleResilientlyAsync(
                 "dbo.sesGetSession",
                 new
                 {
                     EventId = eventId,
                     UserId = this.GetUserId(),
                 },
                 CommandType.StoredProcedure,
                 (Dapper.SqlMapper.GridReader reader) =>
                 {
                     // The order of the recordsets does matter.
                     messages = reader.Read<dynamic>();
                     session = reader.Read<dynamic>().Single();
                 });

            return Ok(new { Session = session, Messages = messages });
        }

        // GET api/sessions/12345/message_count
        [Route("{id:int}/message_count")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetMessageCount(int id)
        {
            const string sql = @"
select dbo.sesGetMessageCount(@SessionId, @UserId) as MessageCount;
";
            int messageCount;
            // This query is executed in pooling. We do not need resiliency here. So avoid overhead, don't use DapperHelper.QueryResilientlyAsync
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                messageCount = (await connection.QueryAsync<int>(sql,
                    new
                    {
                        SessionId = id,
                        UserId = this.GetUserId(),
                    }))
                    .Single();
            }

            return Ok(messageCount);
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

        // POST /api/sessions/
        [Route("")]
        public async Task<IHttpActionResult> PostSession([FromBody] JObject json)
        {
            TimeUtils.ThrowIfClientTimeIsAmbiguous((string)json["localTime"], (int)json["localTimezoneOffset"]);

            // angular-bootstrap-datetimepicker sends values as UTC
            // There is constraint [Start]<[End] on dbo.relScheduleEvents .
            var start = (DateTime)json["start"];
            var end = start.AddMinutes((int)json["duration"]);

            var hostUserId = (int)json["userId"]; // who will confirm
            var guestUserId = this.GetUserId(); // who requests and pays
            if (hostUserId == guestUserId)
            {
                return BadRequest("Wrong session partner.");
            }

            var priceStr = ((string)json["price"]).Trim().Replace(',', '.');
            decimal price;
            var parsed = decimal.TryParse(priceStr, out price);
            if (!parsed)
            {
                return BadRequest(priceStr);
            }

            var message = (string)json["message"];
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            var messageExtId = await SaveMessageAndGetExtId(message);

            // This operation is NOT idempotent.
            DapperHelper.ExecuteResiliently("dbo.sesRequestSession", new
            {
                GuestUserId = guestUserId, // who requests and pays
                HostUserId = hostUserId, // who will confirm
                Start = start,
                End = end,
                Price = price,
                MessageExtId = messageExtId,
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/sessions/12345?action=abcdefg
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PostAction(int id, string action, [FromBody] JObject json)
        {
            string procName = null;
            switch (action)
            {
                case "Confirm":
                    procName = "dbo.sesConfirmSession";
                    break;
                case "Cancel":
                    procName = "dbo.sesCancelSession";
                    break;
                case "Dispute":
                    procName = "dbo.sesDisputeSession";
                    break;
                case "SendMessage":
                    procName = "dbo.sesPostSessionMessage";
                    break;
                default:
                    return BadRequest(action);
            };

            var message = (string)json["message"];
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            var messageExtId = await SaveMessageAndGetExtId(message);

            await DapperHelper.ExecuteResilientlyAsync(procName,
                new
                {
                    SessionId = id,
                    UserId = this.GetUserId(),
                    MessageExtId = messageExtId,
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/sessions/vacant_time
        [Route("vacant_time")]
        public async Task<IHttpActionResult> PutVacantTime([FromBody] JObject value)
        {
            // angular-bootstrap-datetimepicker sends values as UTC
            // There is constraint [Start]<[End] in dbo.sesScheduleEvents .
            // This operation is idempotent.
            await DapperHelper.ExecuteResilientlyAsync("dbo.sesInsertVacantTime",
                new
                {
                    UserId = this.GetUserId(),
                    Start = (DateTime)value["start"],
                    End = (DateTime)value["end"],
                },
                CommandType.StoredProcedure);

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

        private async Task<string> SaveMessageAndGetExtId(string message)
        {
            string extId = null;
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            if (!String.IsNullOrWhiteSpace(message))
            {
                extId = KeyUtils.GetTwelveBase32Digits();
                var entity = new UserMessageEntity
                {
                    PartitionKey = extId,
                    RowKey = String.Empty,
                    Text = message,
                };
                await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.UserMessages, entity);
            }
            return extId;
        }

        private void ConvertFullCalendarDates(string start, string end, string localTime, int localTimezoneOffset, out DateTime startDate, out DateTime endDate)
        {
            /* FullCalendar passes Start and End as midnights without a timezone. 
             * In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the exact midnight local time.
             * We use the client-side TimezoneOffset to calculate times */
            TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

            DateTime startDay, endDay;
            if ((!DateTime.TryParse(start, null, DateTimeStyles.RoundtripKind, out startDay))
                ||
                (!DateTime.TryParse(end, null, DateTimeStyles.RoundtripKind, out endDay)))
            {
                throw new ArgumentException("Date is wrong.");
            }

            startDate = startDay.AddMinutes(localTimezoneOffset);
            endDate = endDay.AddMinutes(localTimezoneOffset);
        }

        private void ConvertDateTimePickerDates(string start, string end, string localTime, int localTimezoneOffset, out DateTime startDate, out DateTime endDate)
        {
            // angular-bootstrap-datetimepicker sends values as UTC
            TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

            if ((!DateTime.TryParse(start, null, DateTimeStyles.RoundtripKind, out startDate))
                ||
                (!DateTime.TryParse(end, null, DateTimeStyles.RoundtripKind, out endDate)))
            {
                throw new ArgumentException("Date is wrong.");
            }
        }


    }
}