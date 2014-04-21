using Microsoft.AspNet.Identity;
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
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/TeachersApi")]
    public class TeachersApiController : ApiController
    {

        // GET /api/TeachersApi/Teachers
        [Route("Teachers")]
        public async Task<IHttpActionResult> GetTeachers()
        {
            var sql = @"
select Id, DisplayName, ReviewRate, SessionRate, dbo.appExtIdToUrl(@BaseUrl, ExtId) as AvatarSmallUrl
from dbo.relGetTeachers(@UserId)
order by DisplayName;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserId = this.GetUserId(),
                    BaseUrl = AzureStorageUtils.GetContainerBaseUrl(AzureStorageUtils.ContainerNames.AvatarsSmall),
                });

            return Ok<object>(result);
        }

        // GET /api/TeachersApi/RandomTeachers/99/0
        [AllowAnonymous]
        [Route("RandomTeachers/{ViewSession:int}/{Bucket:int}")]
        public async Task<IHttpActionResult> GetRandomTeachers(int viewSession, int bucket)
        {
            //ODataQueryOptions<string> queryOptions
            //queryOptions.Validate(new ODataValidationSettings());
            //queryOptions.ApplyTo(null);

            const string sql = @"
select Id, DisplayName, ReviewRate, SessionRate, Announcement, dbo.appExtIdToUrl(@BaseUrl, ExtId) as AvatarLargeUrl 
from dbo.relGetRandomTeachers (@ViewSession, @Bucket) 
order by RowNumber;
";
            var result = await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    ViewSession = viewSession,
                    Bucket = bucket,
                    BaseUrl = AzureStorageUtils.GetContainerBaseUrl(AzureStorageUtils.ContainerNames.AvatarsLarge),
                });

            return Ok<object>(result);
        }

        // PUT /api/TeachersApi/Teachers/1
        [Route("Teachers/{id:int}")]
        public IHttpActionResult PutTeacher(int id)
        {
            // This operation is idempotent.
            DapperHelper.ExecuteResiliently("dbo.relAddLearnerToTeacher", new
            {
                LearnerUserId = this.GetUserId(),
                TeacherUserId = id,
            },
                CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE /api/TeachersApi/Teachers/1
        [Route("Teachers/{id:int}")]
        public IHttpActionResult DeleteTeacher(int id)
        {
            var sql = @"
delete dbo.relLearnersTeachers
    where LearnerUserId = @UserId and TeacherUserId = @TeacherUserId;
";
            var rowsAffected = DapperHelper.ExecuteResiliently(sql, new
            {
                UserId = this.GetUserId(),
                TeacherUserId = id,
            });

            return StatusCode(rowsAffected > 0 ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }


    }
}
