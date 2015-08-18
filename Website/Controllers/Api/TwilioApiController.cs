using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

// devaenglisharium.ddns.net
// +https://demo.twilio.com/welcome/voice/

namespace Runnymede.Website.Controllers.Api
{
    public static class XmlUtils
    {
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        // XDocument.ToString() never emits an XML declaration. 
        // XDocument.Save(XmlWriter) writes a declaration, but ignores "utf-8" in the parameter and always writes "utf-16" (that's correct since it writes to a C# string).
        // We use Utf8StringWriter as a hack.
        public static string ToWebString(this XDocument doc)
        {
            doc.Declaration = new XDeclaration("1.0", "utf-8", null);
            var writer = new Utf8StringWriter();
            doc.Save(writer, SaveOptions.None);
            return writer.ToString();
        }
    }

    [RoutePrefix("api/twilio")]
    public class TwilioApiController : ApiController
    {

        // GET api/twilio/hello-monkey
        [Route("hello-monkey")]
        public IHttpActionResult GetHelloMonkey(string from)
        {
            var action = from == "+16477711715" ? "Accept" : "Reject";

            var doc = new XDocument(
                new XElement("Response",
                    new XElement("Say", "Action: " + action + "."),
                    //new XElement("Play", "http://demo.twilio.com/hellomonkey/monkey.mp3"),
                    new XElement("Gather",
                        new XAttribute("numDigits", 1),
                        new XAttribute("action", "hello-monkey-handle-key"),
                        new XAttribute("method", "GET"),
                        new XElement("Say", "To speak with a real monkey, press 1. Press 2 to record your own monkey howl. Press any other key to start over.")
                        )
                    )
                    );
            var text = doc.ToWebString();

            WriteDebug(Request.RequestUri.AbsoluteUri);

            return new RawStringResult(this, text, RawStringResult.TextMediaType.Xml);
        }

        public class HandleKeyFormModel
        {
            public int Digits { get; set; }
        }

        // POST api/twilio/hello-monkey-handle-key
        //[HttpPost]
        [Route("hello-monkey-handle-key")]
        public IHttpActionResult GetHelloMonkeyHandleKey(int digits)
        //public IHttpActionResult GetHelloMonkeyHandleKey(HandleKeyFormModel form)
        {
            //var digits = form.Digits;
            XDocument doc;
            if (digits == 1)
            {
                doc = new XDocument(
                    new XElement("Response",
                        new XElement("Say", "Record your monkey howl after the tone."),
                        new XElement("Record",
                            new XAttribute("maxLength", 10),
                            new XAttribute("action", "hello-monkey-handle-recording"),
                            new XAttribute("method", "GET")
                        )
                        )
                    );
            }
            else
            {
                doc = new XDocument(
                    new XElement("Response",
                        new XElement("Redirect")
                        )
                    );
            }
            var text = doc.ToWebString();

            WriteDebug(Request.RequestUri.AbsoluteUri);
           // WriteDebug(JavaScriptConvert.Serialize(form).ToString());

            return new RawStringResult(this, text, RawStringResult.TextMediaType.Xml);
        }

        public class HandleRecordingFormModel
        {
            public string RecordingUrl { get; set; }
        }

        // POST api/twilio/hello-monkey-handle-recording
        //[HttpPost]
        [Route("hello-monkey-handle-recording")]
        //public IHttpActionResult GetHelloMonkeyHandleRecording(HandleRecordingFormModel form)
        public IHttpActionResult GetHelloMonkeyHandleRecording(string recordingUrl)
        {
            var doc = new XDocument(
                new XElement("Response",
                    new XElement("Say", "Thanks for howling... take a listen to what you howled."),
                    new XElement("Play", recordingUrl),
                    new XElement("Say", "Goodbye.")
                    )
                );
            var text = doc.ToWebString();

            WriteDebug(Request.RequestUri.AbsoluteUri);
            //WriteDebug(JavaScriptConvert.Serialize(form).ToString());

            return new RawStringResult(this, text, RawStringResult.TextMediaType.Xml);
        }

        private void WriteDebug(string text)
        {
            var t = String.Format("{1}{0}{2}{0}{3}{0}{1}", Environment.NewLine, "-------------------------------------------------------------", DateTime.UtcNow.ToString("u"), text);
            Debug.WriteLine(t);
        }

    }
}
