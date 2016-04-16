using Newtonsoft.Json.Linq;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [RoutePrefix("api/lucky_you")]
    public class LuckyYouApiController : ApiController
    {

        // GET: /api/lucky_you/digits
        [Route("digits")]
        public async Task<IHttpActionResult> GetDigits()
        {
            const string sql = @"
select [Date], Digits from dbo.lckGetDigits();
";
            var digits = await DapperHelper.QueryResilientlyAsync<dynamic>(sql);
            return Ok(digits);
        }

        // POST: /api/lucky_you/entry
        //        [Route("entry")]
        //        public async Task<IHttpActionResult> PostEntry([FromBody] JObject value)
        //        {
        //            var sql = @"
        //merge dbo.lckEntries as Trg
        //using (values(cast(sysutcdatetime() as smalldatetime), @Email)) as Src ([Date], Email)
        //	on Trg.[Date] = Src.[Date] and Trg.Email = Src.Email
        //when not matched then
        //	insert ([Date], Email, Digits, UserId, IpAddress, UserAgent, ExtId)
        //		values (Src.[Date], @Email, @Digits, @UserId, @IpAddress, @UserAgent, @ExtId);
        //";
        //            await DapperHelper.ExecuteResilientlyAsync(sql, new
        //            {
        //                Email = (string)value["email"],
        //                Digits = (string)value["digits"],
        //                UserId = this.IsAuthenticated() ? this.GetUserId() : default(int?),
        //                IpAddress = Request.GetOwinContext().Request.RemoteIpAddress,
        //                UserAgent = String.Join(" ", Request.Headers.GetValues("User-Agent")),
        //                ExtId = this.GetExtId(),
        //            });

        //            return StatusCode(HttpStatusCode.NoContent);
        //        }
        //

        [Route("new_rates")]
        // GET: /api/lucky_you/new_rates?insert=true
        public async Task<IHttpActionResult> GetRates(bool insert = false)
        {
            var helper = new LuckyYouHelper();
            var rates = await helper.ProcessRates();

            // If there is a rate for the date in the table, do not insert it again.
            const string sql = @"
select [Date] from dbo.lckGetDigits();
";
            var knownDates = await DapperHelper.QueryResilientlyAsync<DateTime>(sql);

            // If we don't display a date, don't insert it.
            var minDate = knownDates.Min();

            var newRates = rates
                .Where(i => ((i.Key >= minDate) && (!knownDates.Any(j => j == i.Key))))
                ;

            if (insert)
            {
                const string sqlInsert = @"
insert into dbo.lckRates ([Date], Position, Currency, Rate) 
values (@Date, @Position, @Currency, @Rate);
";
                foreach (var date in newRates)
                {
                    var parameters = date.Value
                        .Select((i, idx) => new
                        {
                            Date = date.Key,
                            Position = idx,
                            Currency = i.Key,
                            Rate = i.Value,
                        })
                        ;
                    foreach (var param in parameters)
                    {
                        await DapperHelper.ExecuteResilientlyAsync(sqlInsert, param);
                    }
                }
            }

            return Ok(newRates);
        }




    }
}
