using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Runnymede.Website.Utils
{
    // +http://www.asp.net/web-api/overview/error-handling/web-api-global-error-handling
    // +https://aspnetwebstack.codeplex.com/SourceControl/latest#src/System.Web.Http/ExceptionHandling/DefaultExceptionHandler.cs
    // +http://www.nesterovsky-bros.com/weblog/2014/03/10/CustomErrorHandlingWithWebAPI.aspx

    /// Defines a global handler for unhandled exceptions.
    public class CustomExceptionHandler : ExceptionHandler
    {
        /// This core method should implement custom error handling, if any. It determines how an exception will be serialized for client-side processing.
        public override void Handle(ExceptionHandlerContext context)
        {
            var requestContext = context.RequestContext;
            var includeErrorDetail = requestContext != null ? requestContext.IncludeErrorDetail : false;
            var config = requestContext.Configuration;
            context.Result = new CustomExceptionResult(context.Exception, includeErrorDetail, config.Services.GetContentNegotiator(), context.Request, config.Formatters);
        }

        /// An implementation of IHttpActionResult interface.
        private class CustomExceptionResult : ExceptionResult
        {
            public CustomExceptionResult(Exception exception, bool includeErrorDetail, IContentNegotiator negotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters) :
                base(exception, includeErrorDetail, negotiator, request, formatters)
            {
            }

            /// Creates an HttpResponseMessage instance asynchronously. This method determines how a HttpResponseMessage content will look like.
            public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var result = ContentNegotiator.Negotiate(typeof(HttpError), Request, Formatters);

                var message = new HttpResponseMessage
                {
                    RequestMessage = Request,
                    StatusCode = result != null ? HttpStatusCode.InternalServerError : HttpStatusCode.NotAcceptable,
                };

                if (result != null)
                {                   
                    string alert = null;

                    if (Exception is SqlException)
                    {
                        // Strip the beginning of the exception message which contains the name of the stored procedure and the argument values. We do not disclose the values to the client.
                        var index = Exception.Message.IndexOf("::"); // The magic separator used within our stored procedures.   
                        if (index >= 0)
                        {
                            alert = Exception.Message.Substring(index + 2).Trim();
                        }
                    }
                    else if (Exception is UserAlertException)
                    {
                        alert = Exception.Message;
                    }
                    // Until we have converted all the exceptions thrown by our code to UserAlertException
                    else
                    {
                        alert = Exception.Message;
                    }

                    var content = new HttpError(Exception, IncludeErrorDetail);

                    if (!String.IsNullOrEmpty(alert))
                    {
                        // Define an additional content field.
                        content.Add("Alert", alert);
                    }

                    // serializes the HttpError instance either to JSON or to XML depend on requested by the client MIME type.
                    message.Content = new ObjectContent<HttpError>(content, result.Formatter, result.MediaType);
                }

                return Task.FromResult(message);
            }
        }
    }




}