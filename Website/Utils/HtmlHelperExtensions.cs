using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Runnymede.Website.Utils
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString HrSeparator(this HtmlHelper html, string gridClass)
        {
            if (String.IsNullOrEmpty(gridClass))
            {
                throw new ArgumentNullException("gridClass");
            }
            return new MvcHtmlString(string.Format(
                @"<div class=""row""><div class=""{0}""><hr class=""app-separator"" /></div></div>
",
                gridClass
                ));
        }

        #region InstructionsContainer

        // We can use BeginInstructionsContainer like Html.BeginForm and when the @using(){} block has ended, the end of the widget's content is output.
        public static IDisposable BeginInstructionsContainer(this HtmlHelper helper)
        {

            /*
                            <i class='fa fa-question-circle app-appblue' style='font-size:28px;position:relative;top:7px;'></i>
 
             */

            const string head = @"
<script type='text/javascript'>
    function toggleHelpCaption(el) {
        var i = '<i class=""fa fa-question-circle fa-fw app-1px-down""></i>'
        if (el.value === 'Show') {
            el.value = 'Hide';
            el.innerHTML = i + ' Hide Help\xA0'
        }
        else {
            el.value = 'Show';
            el.innerHTML = i + ' Show Help'
        }
    }
</script>
<div class='row'><div class='col-sm-12'><hr/></div></div>
<div class='row'>
    <div class='col-sm-2'>
        <button class='btn btn-default btn-xs app-1px-down' style='margin-left:25px;' data-toggle='collapse' data-target='#instructions' value='Show' onclick='toggleHelpCaption(this)'>
            <i class='fa fa-question-circle fa-fw'></i> Show Help
        </button>
    </div>
    <div id='instructions' class='col-sm-8 collapse'>
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
                    this.helper.ViewContext.Writer.Write("</div></div>");
                }
            }
        }

        #endregion

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
            return htmlHelper.ActionLink(linkText, actionName, controllerName, protocol, null, null, null, htmlAttributes);
        }

        public static string ActionWithProtocol(this UrlHelper urlHelper, string actionName, string controllerName, string protocol)
        {
            //return urlHelper.Action(actionName, controllerName, null, protocol, GetHostNameForProtocol(protocol, urlHelper.RequestContext.HttpContext));
            return urlHelper.Action(actionName, controllerName, null, protocol, null);
        }

    } // end of class HtmlHelperExtensions

    /// <summary>
    /// Uses a custom Json.NET serializer to turn .NET objects into JavaScript literal representation.
    /// Note that the output is not valid JSON because the property names aren't wrapped in quotes
    /// </summary>
    public static class JavaScriptConvert
    {
        public static IHtmlString Serialize(object value)
        {
            return new HtmlString(JsonUtils.SerializeAsJavaScript(value));
        }
    }
}