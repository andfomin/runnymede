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
    [RoutePrefix("api/storytelling")]
    public class StorytellingApiController : ApiController
    {
        [Route("blobName/{rootCategory}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetVideoBlobName(string rootCategory)
        {
            const string sql = @"
select top (1) Id, RootCategory, BlobName, Description 
from dbo.stoVideos
where RootCategory = @RootCategory
order by Id desc;
";
            var video = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { RootCategory = rootCategory, }))
                .Single()
                ;
            return Ok(video);
        }


    }
}
