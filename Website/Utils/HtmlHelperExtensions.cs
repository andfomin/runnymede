using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace Runnymede.Website.Utils
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString HrSeparator(this HtmlHelper html, string gridClass)
        {
            if (string.IsNullOrEmpty(gridClass))
            {
                throw new ArgumentNullException("gridClass");
            }
            return new MvcHtmlString(string.Format(
                @"<div class=""row""><div class=""{0}""><hr class=""app-separator"" /></div></div>
",
                gridClass
                ));
        }

        // We can use BeginInstructionsContainer like Html.BeginForm and when the @using(){} block has ended, the end of the widget's content is output.
        public static IDisposable BeginInstructionsContainer(this HtmlHelper helper)
        {
            const string head = @"
<script type='text/javascript'>
    function toggleHelpCaption(el) {
        if (el.value === 'Show Help')
            el.value = 'Hide Help\xA0';
        else
            el.value = 'Show Help';
        }
</script>
<div class='row'><div class='col-sm-12'><hr class='app-separator' /></div></div>
<div class='row'>
    <div class='col-sm-2'>
        <small>
            <input type='button' class='btn btn-default btn-xs' data-toggle='collapse' data-target='#instructions' value='Show Help' onclick='toggleHelpCaption(this)' />
        </small>
    </div>
    <div id='instructions' class='col-sm-8 collapse'>
        <ul>
";

            helper.ViewContext.Writer.Write(head);
            return new InstructionsContainerEnd(helper);
        }

        class InstructionsContainerEnd : IDisposable
        {
            private HtmlHelper helper;
            private bool disposed;

            public InstructionsContainerEnd(HtmlHelper helper)
            {
                this.helper = helper;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.helper.ViewContext.Writer.Write("</ul></div></div>");
                }
            }
        }

        private static bool IsDebug()
        {
#if DEBUG
            return true;
#else
      return false;
#endif
        }

        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
            return IsDebug();
            ////@if (HttpContext.Current.IsDebuggingEnabled) { // Debug mode is enabled in Web.config. }
        }

        public static MvcHtmlString ActionLinkWithProtocol(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, object htmlAttributes)
        {
            //return htmlHelper.ActionLink(linkText, actionName, controllerName, protocol, GetHostNameForProtocol(protocol, htmlHelper.ViewContext.HttpContext), null, null, htmlAttributes);
            return htmlHelper.ActionLink(linkText, actionName, controllerName, null, null, null, null, htmlAttributes);
        }

        public static string ActionWithProtocol(this UrlHelper urlHelper, string actionName, string controllerName, string protocol)
        {
            //return urlHelper.Action(actionName, controllerName, null, protocol, GetHostNameForProtocol(protocol, urlHelper.RequestContext.HttpContext));
            return urlHelper.Action(actionName, controllerName, null, null, null);
        }

        private static string GetHostNameForProtocol(string protocol, HttpContextBase httpContext)
        {
            // ??? IIS and ASP.NET are aware of the HTTP port through the project properties. There is no HTTPS port setting in the project properties.
            if (IsDebug())
            {
                var url = httpContext.Request.Url;
                var host = url.Host;
                var domain = host.Split(':').First();
                var scheme = url.Scheme;

                var httpPort = CustomController.GetAppSetting("HttpPort", "80");
                var httpsPort = CustomController.GetAppSetting("HttpsPort", "443");

                var isHttps = protocol == "https";
                var isCurrentlyHttps = host.Contains(httpsPort);

                return domain + ((protocol == scheme) ? "" : (":" + (isHttps ? httpsPort : httpPort)));
            }
            else
                return null;
        }

    } // end of class HtmlHelperExtensions
}