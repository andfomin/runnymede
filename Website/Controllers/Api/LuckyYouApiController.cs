using Newtonsoft.Json.Linq;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
        [RoutePrefix("api/luckyyou")]
    public class LuckyYouApiController : ApiController
    {

        // POST: /api/luckyyou/entry
        [Route("entry")]
        public async Task<IHttpActionResult> PostEntry([FromBody] JObject value)
        {
            var sql = @"
merge dbo.lckEntries as Trg
using (values(cast(sysutcdatetime() as smalldatetime), @Email)) as Src ([Date], Email)
	on Trg.[Date] = Src.[Date] and Trg.Email = Src.Email
when not matched then
	insert ([Date], Email, Digits, UserId, IpAddress, UserAgent, ExtId)
		values (Src.[Date], @Email, @Digits, @UserId, @IpAddress, @UserAgent, @ExtId);
";
            await DapperHelper.ExecuteResilientlyAsync(sql, new {
                Email = (string)value["email"],
                Digits = (string)value["digits"],
                UserId = this.IsAuthenticated() ? this.GetUserId() : default(int?),
                IpAddress = Request.GetOwinContext().Request.RemoteIpAddress,
                UserAgent = String.Join(" ", Request.Headers.GetValues("User-Agent")),
                ExtId = this.GetExtId(),
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
