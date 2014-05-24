using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Runnymede.Website.Utils;
using System.IO;

namespace Runnymede.Website.Controllers.Api
{
    // We allow anonymous users to post. And we respect the authorized users' identity.
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/ResourcesApi")]
    public class ResourcesApiController : ApiController
    {
        // +https://developers.google.com/youtube/v3/docs/videos/list
        // id, snippet, contentDetails, liveStreamingDetails, recordingDetails, statistics, status, topicDetails
        // +http://www.freebase.com/m/02h40lc - English Language 
        // +http://www.freebase.com/m/0jt6_33 - English as a second or foreign language
        // 01b5n5 - Vocabulary. 01h8n0 - Conversation

        /// Use ASP.NET Identity to see if the user is logged in. If they are, we can get their User Id (blank otherwise)
        private bool TryGetUserId(out int userId)
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            userId = isAuthenticated
                ? this.GetUserId()
                : 0;
            return isAuthenticated;
        }

        // GET /api/ResourcesApi/Url
        [Route("Resource")]
        public IHttpActionResult PostResource([FromBody] JObject value)
        {
            var url = (string)value["url"];





            return Ok();
        }



    }
}
