using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Query;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Dapper;
using System.Data.Entity.SqlServer;
using System.Threading;
using System.Data;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/RelationshipsApi")]
    public class RelationshipsApiController : ApiController
    {

        // GET api/RelationshipsApi/FavoriteTutors
        [Route("FavoriteTutors")]
        //public async Task<IEnumerable<object>> GetTutors()
        public async Task<IHttpActionResult> GetFavoriteTutors()
        {
            var sql = @"
select Id, DisplayName, RateARec 
from dbo.relGetRelatedTutors(@UserId)
order by DisplayName;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserId = this.GetUserId(),
                });

            //return result;
            return Ok<object>(result);
        }


        // GET api/RelationshipsApi/RandomTutors/99/0
        [AllowAnonymous]
        [Route("RandomTutors/{session:int}/{bucket:int}")]
        public async Task<IHttpActionResult> GetRandomTutors(int session, int bucket)
        {
            //ODataQueryOptions<string> queryOptions
            //queryOptions.Validate(new ODataValidationSettings());
            //queryOptions.ApplyTo(null);

            const string sql = @"
select Id, DisplayName, RateARec from dbo.relGetRandomTutors (@Session, @Bucket);
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    Session = session,
                    Bucket = bucket,
                });

            //return result;
            return Ok<object>(result);
        }

        // PUT api/RelationshipsApi/1
        public IHttpActionResult Put(int id)
        {
            const string sql = @"
exec dbo.relEnsureLearnerTutorRelation @LearnerUserId = @UserId, @TutorUserId = @TutorUserId;

update dbo.relLearnersTutors set L2TDate = getutcdate()
	where LearnerUserId = @UserId and TutorUserId = @TutorUserId and L2TDate is null;
";


            DapperHelper.ExecuteResiliently(sql, new
                {
                    UserId = this.GetUserId(),
                    TutorUserId = id,
                });

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE api/RelationshipsApi/1
        public IHttpActionResult Delete(int id)
        {
            var sql = @"
update dbo.relLearnersTutors set L2TDate = null
    where LearnerUserId = @UserId and TutorUserId = @TutorUserId;
";
            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
            {
                UserId = this.GetUserId(),
                TutorUserId = id,
            });

            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);

        }
    }
}