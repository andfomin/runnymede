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

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/SessionsApi")]
    public class SessionsApiController : ApiController
    {
        private class Types
        {
            // ScheduleEvent types. Correspond to dbo.relScheduleEventTypes and App.Model.ScheduleEvent.Types in utils-schedule.ts
            public const string Offered = "OFFR"; // Offer
            public const string Revoked = "ROFR"; // Revoked Offer
            public const string Requested = "RQSN"; // Requested session
            public const string Confirmed = "CFSN"; // Confirmed session 
            public const string CancelledUS = "CSUS"; // Cancelled Session, by User, i.e. teacher
            public const string CancelledSU = "CSSU"; // Cancelled Session, by SecondUser, i.e. learner
            public const string Closed = "CLSN"; // Closed session 
            public const string Disputed = "DSSN"; // Disputed session 
        }

        // GET api/SessionsApi/MillisSinceEpoch
        [Route("MillisSinceEpoch")]
        public Int64 GetMillisSinceEpoch()
        {
            // // Calculate the difference on the client between the local time and provided time. It is used to show a warning on the page if the local clock is wrong.
            // JavaScript uses milliseconds since the Unix epoch. 
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var millisecondsSinceEpoch = timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
            return millisecondsSinceEpoch;
        }

        private void convertDates(string start, string end, string localTime, int localTimezoneOffset, out DateTime startDate, out DateTime endDate)
        {
            /* FullCalendar passes Start and End as midnights without a timezone. 
             * In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the exact midnight local time.
             * We use the client-side TimezoneOffset to calculate times */
            if (!LoggingUtils.ClientTimeIsOk(localTime, localTimezoneOffset))
            {
                throw new ArgumentException(LoggingUtils.AmbiguousTimezoneOffset);
            }

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

        private async Task<IEnumerable<ScheduleEventDto>> InternalGetOwnSchedule(DateTime start, DateTime end)
        {
            var sql = string.Format(@"
select Id, Start, [End], [Type]
from dbo.relScheduleEvents
where (UserId = @UserId or SecondUserId = @UserId)
    and Start <= @End
    and [End] >= @Start
    and [Type] in ('{0}','{1}','{2}','{3}','{4}','{5}','{6}');
",
    Types.Offered, Types.Requested, Types.Confirmed, Types.CancelledSU, Types.CancelledUS, Types.Disputed, Types.Closed
 );
            return await DapperHelper.QueryResilientlyAsync<ScheduleEventDto>(sql, new
            {
                UserId = this.GetUserId(),
                Start = start,
                End = end,
            });
        }

        private async Task<IEnumerable<ScheduleEventDto>> InternalGetUserSchedule(int userId, DateTime start, DateTime end)
        {
            // We show not-expired events for the first user. We show all events for the second user.
            var sql = string.Format(@"
declare @Now smalldatetime = sysutcdatetime();

select
    case when SecondUserId = @SecondUserId then Id else null end as Id,
    Start, [End], [Type]
from dbo.relScheduleEvents
where UserId = @UserId
    and Start < @End
    and [End] > @Start
    and (([Type] in ('{0}','{1}','{2}') and [End] > @Now) or SecondUserId = @SecondUserId);
",
    Types.Offered, Types.Requested, Types.Confirmed
  );
            return await DapperHelper.QueryResilientlyAsync<ScheduleEventDto>(sql, new
            {
                UserId = userId,
                SecondUserId = this.GetUserId(),
                Start = start, // The dates come as UTC.
                End = end,
            });
        }

        // GET api/SessionsApi/OwnSchedule
        [Route("OwnSchedule")]
        public async Task<IHttpActionResult> GetOwnSchedule(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            convertDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);
            var events = await InternalGetOwnSchedule(startDate, endDate);
            return Ok(events);
        }

        // GET api/SessionsApi/UserSchedule/12345
        [Route("UserSchedule/{userId:int}")]
        public async Task<IHttpActionResult> GetUserSchedule(int userId, string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            convertDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);
            var events = await InternalGetUserSchedule(userId, startDate, endDate);
            return Ok(events);
        }

        // GET api/SessionsApi/UsersWithOffers
        [Route("UsersWithOffers")]
        public async Task<IHttpActionResult> GetUsersWithOffers(string date, int startHour, int endHour, string localTime, int localTimezoneOffset)
        {
            // The Date and Hours values come from the custom filter. The date comes as the local midnight expressed for the UTC timezone.
            if (!LoggingUtils.ClientTimeIsOk(localTime, localTimezoneOffset))
            {
                return BadRequest(LoggingUtils.AmbiguousTimezoneOffset);
            }

            DateTime dateValue;
            if (!DateTime.TryParse(date, null, DateTimeStyles.RoundtripKind, out dateValue))
            {
                return BadRequest("Date is wrong.");
            }

            var sql = string.Format(@"
select Id, DisplayName, IsTeacher, SessionRate, Announcement
from dbo.appUsers
where Id in (
	select UserId
	from dbo.relScheduleEvents
	where Start < @End 
		and [End] > @Start
        and [End] > sysutcdatetime()
        and [Type] = '{0}'
);
",
    Types.Offered
 );
            var users = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new
            {
                Start = dateValue.AddHours(startHour), 
                End = dateValue.AddHours(endHour),
            }));

            return Ok(users);
        }

        // GET api/SessionsApi/UserOffers/12345
        [Route("UserOffers/{userId:int}")]
        public async Task<IHttpActionResult> GetUserOffers(int userId, string date, int startHour, int endHour)
        {
            // This request comes from the offer search page, from an item on the list of the found users. The date comes as the local midnight expressed for the UTC timezone.
            DateTime dateValue;
            if (!DateTime.TryParse(date, null, DateTimeStyles.RoundtripKind, out dateValue))
            {
                return BadRequest("Date is wrong.");
            }

            var start = dateValue.AddHours(startHour);
            var end = dateValue.AddHours(endHour);
            var events = await InternalGetUserSchedule(userId, start, end);
            return Ok(events);
        }

        // GET api/SessionsApi/SessionDetails/12345
        [Route("SessionDetails/{id:int}")]
        public IHttpActionResult GetSessionDetails(int id)
        {
            ScheduleEventDto scheduleEvent = null;
            IEnumerable<MessageDto> messages = null;

            DapperHelper.QueryMultipleResiliently(
                "dbo.relGetSessionDetails",
                new
                {
                    EventId = id,
                    UserId = this.GetUserId(),
                },
                CommandType.StoredProcedure,
                (Dapper.SqlMapper.GridReader reader) =>
                {
                    scheduleEvent = reader.Read<ScheduleEventDto>().SingleOrDefault();
                    messages = reader.Read<MessageDto>();
                });

            return Ok(new { Event = scheduleEvent, Messages = messages });
        }

        // GET api/SessionsApi/SessionMessageCount/12345
        [Route("SessionMessageCount/{id:int}")]
        public async Task<IHttpActionResult> GetSessionMessageCount(int id)
        {
            int messageCount;
            // We do not need resiliency here, so avoid overhead.
            using (var connection = DapperHelper.GetOpenConnection())
            {
                messageCount = (await connection.QueryAsync<int>("dbo.relGetSessionMessageCount", new
                {
                    EventId = id,
                    UserId = this.GetUserId(),
                },
                commandType: CommandType.StoredProcedure))
                .Single();
            }

            return Ok(messageCount);
        }

        // PUT /api/SessionsApi/Offer
        [Route("Offer")]
        public IHttpActionResult PutOffer([FromBody] JObject value)
        {
            // angular-bootstrap-datetimepicker sends values as UTC
            // There is constraint [EventStart]<[EventEnd] on dbo.relSchedules .
            // This operation is idempotent.
            DapperHelper.ExecuteResiliently("dbo.relOfferSession", new
            {
                UserId = this.GetUserId(),
                Start = (DateTime)value["start"],
                End = (DateTime)value["end"],
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE /api/SessionsApi/Offer
        [Route("Offer/{id:int}")]
        public IHttpActionResult DeleteOffer(int id)
        {
            DapperHelper.ExecuteResiliently("dbo.relRevokeSessionOffer", new
            {
                UserId = this.GetUserId(),
                Id = id,
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/SessionsApi/Session
        [Route("Session")]
        public IHttpActionResult PostSession([FromBody] JObject json)
        {
            if (!LoggingUtils.ClientTimeIsOk((string)json["localTime"], (int)json["localTimezoneOffset"]))
            {
                return BadRequest(LoggingUtils.AmbiguousTimezoneOffset);
            }

            // angular-bootstrap-datetimepicker sends values as UTC
            // There is constraint [Start]<[End] on dbo.relScheduleEvents .
            var start = (DateTime)json["start"];
            var end = start.AddMinutes((int)json["duration"]);

            // This operation is NOT idempotent.
            DapperHelper.ExecuteResiliently("dbo.relRequestSession", new
            {
                UserId = (int)json["userId"], // teacher
                SecondUserId = this.GetUserId(), // learner
                Start = start,
                End = end,
                Message = (string)json["message"],
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/SessionsApi/Message
        [Route("Message")]
        public IHttpActionResult PostMessage([FromBody] JObject json)
        {
            var message = (string)json["message"];
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest();
            }

            DapperHelper.ExecuteResiliently("dbo.relPostSessionMessage", new
            {
                EventId = (int)json["eventId"],
                UserId = this.GetUserId(),
                Message = message,
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/SessionsApi/Action
        [Route("Action")]
        public IHttpActionResult PostAction([FromBody] JObject json)
        {
            var action = (string)json["action"];
            string spName = "";
            switch (action)
            {
                case Types.Confirmed:
                    spName = "dbo.relConfirmSession";
                    break;
                case Types.CancelledUS + Types.CancelledSU:
                    spName = "dbo.relCancelSession";
                    break;
                case Types.Disputed:
                    spName = "dbo.relDisputeSession";
                    break;
                default:
                    return BadRequest(action);
            };

            DapperHelper.ExecuteResiliently(spName, new
            {
                EventId = (int)json["eventId"],
                UserId = this.GetUserId(),
                Message = (string)json["message"],
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }



    }
}