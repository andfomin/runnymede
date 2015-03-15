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
    [RoutePrefix("api/converstions")]
    [Authorize]
    public class ConverstionsApiController : ApiController
    {
        private const int MaxPresenceAgeHours = 48;

        // GET: /api/converstions/
        [Route("")]
        public async Task<IHttpActionResult> GetOffers()
        {
            var minCreateTime = DateTime.UtcNow.AddHours(-MaxPresenceAgeHours);
            var userId = this.GetUserId();

            const string sqlOffered = @"
select cast(case when
	    exists (
		    select *
		    from dbo.conOffers
		    where CreateTime > @MinCreateTime
			    and UserId = @UserId	
	    )
	then 1 else 0 end as bit);
";
            var offered = (await DapperHelper.QueryResilientlyAsync<bool>(sqlOffered, new { MinCreateTime = minCreateTime, UserId = userId, })).Single();

            object users = null;

            if (offered)
            {
                const string sqlOffers = @"
select Id, DisplayName, Announcement, SkypeName, InvitationTo, InvitationFrom
from dbo.conGetOffers(@MinCreateTime, @UserId)
order by CreateTime desc;
";
                users = await DapperHelper.QueryResilientlyAsync<dynamic>(sqlOffers, new { MinCreateTime = minCreateTime, UserId = userId, });

            }

            return Ok(new { Offered = offered, Users = users, });
        }

        // POST /api/converstions/2000000001
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PostOffer(int id)
        {
            const string sql = @"
insert dbo.conOffers(CreateTime, UserId, ToUserId) values (sysutcdatetime(), @UserId, @ToUserId);
";
            await DapperHelper.ExecuteResilientlyAsync(sql, new { UserId = this.GetUserId(), ToUserId = id, });
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
