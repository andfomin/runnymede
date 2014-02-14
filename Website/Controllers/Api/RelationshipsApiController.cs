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
using Newtonsoft.Json.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/RelationshipsApi")]
    public class RelationshipsApiController : ApiController
    {

        // GET /api/RelationshipsApi/Tutors
        [Route("Tutors")]
        public async Task<IHttpActionResult> GetTutors()
        {
            var sql = @"
select Id, DisplayName, Skype, Rate 
from dbo.relGetTutors(@UserId)
order by DisplayName;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserId = this.GetUserId(),
                });

            return Ok<object>(result);
        }

        // GET /api/RelationshipsApi/TutorLearners
        [Route("TutorLearners")]
        public async Task<IHttpActionResult> GetTutorLearners()
        {
            var sql = @"
select Id, DisplayName, Skype 
from dbo.relGetTutorLearners(@UserId)
order by DisplayName;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserId = this.GetUserId(),
                });

            return Ok<object>(result);
        }

        // GET /api/RelationshipsApi/RandomTutors/99/0
        [AllowAnonymous]
        [Route("RandomTutors/{session:int}/{bucket:int}")]
        public async Task<IHttpActionResult> GetRandomTutors(int session, int bucket)
        {
            //ODataQueryOptions<string> queryOptions
            //queryOptions.Validate(new ODataValidationSettings());
            //queryOptions.ApplyTo(null);

            const string sql = @"
select Id, DisplayName, Rate, Announcement 
from dbo.relGetRandomTutors (@Session, @Bucket) 
order by RowNumber;
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

        // PUT /api/RelationshipsApi/Tutors/1
        [Route("Tutors/{id:int}")]
        public IHttpActionResult PutTutor(int id)
        {
            // This operation is idempotent.
            DapperHelper.ExecuteResiliently("dbo.relAddLearnerToTutor", new
                {
                    LearnerUserId = this.GetUserId(),
                    TutorUserId = id,
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE /api/RelationshipsApi/Tutors/1
        [Route("Tutors/{id:int}")]
        public IHttpActionResult DeleteTutor(int id)
        {
            var sql = @"
delete dbo.relLearnersTutors
    where LearnerUserId = @UserId and TutorUserId = @TutorUserId;
";
            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
            {
                UserId = this.GetUserId(),
                TutorUserId = id,
            });

            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        // GET /api/RelationshipsApi/SkypeDirectory
        [Route("SkypeDirectory")]
        public async Task<IHttpActionResult> GetSkypeDirectory()
        {
            var sql = @"
select Skype, Announcement, cast(case when UserId = @UserId then 1 else 0 end as bit) as IsSelf 
from dbo.relGetSkypeDirectory()
order by RowNumber;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { UserId = this.GetUserId() });

            return Ok<object>(result);
        }

        // POST: /api/RelationshipsApi/SkypeDirectory
        [Route("SkypeDirectory")]
        public IHttpActionResult PostSkypeDirectory([FromBody] JObject value)
        {
            DapperHelper.ExecuteResiliently("dbo.relJoinSkypeDirectory", new
                {
                    UserId = this.GetUserId(),
                    Skype = (string)value["skype"],
                    Announcement = (string)value["announcement"],
                },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE /api/RelationshipsApi/SkypeDirectory
        [Route("SkypeDirectory")]
        public IHttpActionResult DeleteSkypeDirectory()
        {
            var sql = @"
update dbo.relSkypeDirectory
set TimeEnd = sysutcdatetime()
where UserId = @UserId
	and TimeEnd is null;
";
            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
            {
                UserId = this.GetUserId(),
            });

            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }







    }
}