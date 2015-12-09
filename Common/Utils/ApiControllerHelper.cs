using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Runnymede.Website.Utils
{
    public class ApiControllerHelper
    {
    }

    /// <summary>
    /// Returns a JSON-wrapped error message politely asking to log in. Returns a 400 BadRequest, not a 401 Unauthorized.
    /// Intended to decorate ApiController methods. Do not use with the MVC Controller.
    /// </summary>
    public class AppPoliteAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"message\":\"Please log in.\"}", Encoding.UTF8, JsonMediaTypeFormatter.DefaultMediaType.MediaType)
                };
            }
        }
    }

}