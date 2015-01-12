using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [RoutePrefix("api/friends")]
    public class FriendsApiController : ApiController
    {

        // GET: /api/friends/
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            const string sql = @"
select Id, DisplayName, Announcement,		
    UserIsActive, FriendIsActive,
    UserRecordingRate, FriendRecordingRate, UserWritingRate, FriendWritingRate, UserSessionRate, FriendSessionRate,
    UserLastContactDate, FriendLastContactDate, UserLastContactType, FriendLastContactType
from dbo.friGetFriends(@UserId)
order by iif(UserLastContactDate > FriendLastContactDate, UserLastContactDate, FriendLastContactDate) desc;
";
            var friends = await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { UserId = this.GetUserId() });
            return Ok(friends);
        }

        // TODO: Is it used or abandoned?
        // GET: /api/friends/active
        [Route("active")]
        public async Task<IHttpActionResult> GetActive()
        {
            const string sql = @"
select Id, DisplayName,  		
    UserRecordingRate, FriendRecordingRate, UserWritingRate, FriendWritingRate, UserSessionRate, FriendSessionRate,
    UserLastContactDate, FriendLastContactDate, UserLastContactType, FriendLastContactType
from dbo.friGetFriends(@UserId)
where UserIsActive = 1 and FriendIsActive = 1;
";
            var friends = await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { UserId = this.GetUserId() });
            return Ok(friends);
        }

        // POST /api/friends/
        [Route("")]
        public async Task<IHttpActionResult> PostFriend([FromBody] JObject value)
        {
            // This operation is not idempotent. There may be a primary key violation on an attempt to add an already existing friendship again.
            await DapperHelper.ExecuteResilientlyAsync("dbo.friAddFriend",
                 new
                 {
                     UserId = this.GetUserId(),
                     Email = (string)value["email"],
                 },
                 CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT /api/friends/review_rate
        [Route("review_rate")]
        public async Task<IHttpActionResult> PutReviewRate([FromBody] JObject value)
        {
            // Parse the writingRate value entered by the user.
            var rateStr = ((string)value["rate"]).Trim().Replace(',', '.');
            // Rates are nullable.
            decimal? rate = null;
            if (!String.IsNullOrEmpty(rateStr))
            {
                decimal parsed;
                if (decimal.TryParse(rateStr, out parsed))
                {
                    rate = parsed;
                }
                else
                    return BadRequest(rateStr);
            }
            // This operation is idempotent.
            await DapperHelper.ExecuteResilientlyAsync("dbo.friUpdateReviewRate",
                new
                {
                    UserId = this.GetUserId(),
                    FriendUserId = (int)value["friendUserId"],
                    ExerciseType = (string)value["exerciseType"],
                    Rate = rate,
                },
                CommandType.StoredProcedure);

            return Ok(new { Rate = rate });
        }

        // PUT /api/friends/active
        [Route("active")]
        public async Task<IHttpActionResult> PutActive([FromBody] JObject value)
        {
            // This operation is idempotent.
            await DapperHelper.ExecuteResilientlyAsync("dbo.friUpdateIsActive",
                new
                {
                    UserId = this.GetUserId(),
                    FriendUserId = (int)value["friendUserId"],
                    IsActive = (bool)value["isActive"],
                },
                CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
