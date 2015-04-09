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
using System.Xml.Linq;

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

        // GET api/sessions
        [Route("")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetSessions(string start, string end, string localTime, int localTimezoneOffset)
        {
            DateTime startDate, endDate;
            ConvertFullCalendarDates(start, end, localTime, localTimezoneOffset, out startDate, out endDate);

            var items = await DapperHelper.QueryResilientlyAsync<SessionDto>(
                this.GetUserIsTeacher() ? "dbo.sesGetSessionsForTeacher" : "dbo.sesGetSessionsForLearner",
                new
                {
                    UserId = this.GetUserId(),
                    Start = startDate,
                    End = endDate,
                },
            CommandType.StoredProcedure);

            return Ok(items);
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
                   .Single();

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

            await DapperHelper.ExecuteResilientlyAsync("dbo.sesBookSession", new
            {
                UserId = this.GetUserId(), // who requests and pays
                Start = (DateTime)json["start"],
                End = (DateTime)json["end"],
                Price = (decimal)json["price"],
                TeacherUserId = (int?)json["teacherUserId"],
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/sessions/12345/message
        [Route("{id:int}/message")]
        public async Task<IHttpActionResult> PostMessage(int id, [FromBody] JObject json)
        {
            var message = (string)json["message"];
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            var messageExtId = await SaveMessageAndGetExtId(message);

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

        //private void ConvertDateTimePickerDates(string start, string end, string localTime, int localTimezoneOffset, out DateTime startDate, out DateTime endDate)
        //{
        //    // angular-bootstrap-datetimepicker sends values as UTC
        //    TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

        //    if ((!DateTime.TryParse(start, null, DateTimeStyles.RoundtripKind, out startDate))
        //        ||
        //        (!DateTime.TryParse(end, null, DateTimeStyles.RoundtripKind, out endDate)))
        //    {
        //        throw new ArgumentException("Date is wrong.");
        //    }
        //}

    }
}