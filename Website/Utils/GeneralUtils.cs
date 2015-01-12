using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Runnymede.Website.Utils
{

    public static class MediaType
    {
        public const string Mp3 = "audio/mpeg";
        public const string Amr = "audio/amr";
        public const string Gpp = "audio/3gpp";
        public const string QuickTime = "video/quicktime";
        public const string Jpeg = MediaTypeNames.Image.Jpeg; // "image/jpeg";
        public const string Octet = MediaTypeNames.Application.Octet;// "application/octet-stream";
        public const string Json = "application/json"; // JsonMediaTypeFormatter.DefaultMediaType.MediaType
        public const string PlainText = MediaTypeNames.Text.Plain; // "text/plain";

        public static string GetExtension(string contentType)
        {
            switch (contentType)
            {
                case Mp3:
                    return "mp3";
                case Amr:
                    return "amr"; // 3ga
                case Gpp:
                    return "3gp"; // 3gpp
                case QuickTime:
                    return "mov";
                case Jpeg:
                    return "jpg";
                default:
                    return null;
            }
        }
    }

    public static class GeneralUtils
    {
        /// <summary>
        ///  Strips out everything except alphanumeric characters and spaces. Normalizes spaces. "словари café _" is OK.
        /// </summary>
        /// <param name="input">Any text. Expects words separated by spaces</param>
        /// <returns>Words separated by spaces</returns>
        public static string SanitizeSpaceSeparatedWords(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return null;
            }
            // Strip out everything except alphanumeric characters and spaces.
            var sanitized = Regex.Replace(input, @"[^\w\ ]", "", RegexOptions.None); // "словари café _" is OK.
            // Normalize spaces.
            return String.Join(" ", sanitized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public static class JsonUtils
    {
        private static JsonSerializerSettings GetGlobalJsonSettings()
        {
            //var settings0 = new JsonSerializerSettings
            //{
            //    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            //    NullValueHandling = NullValueHandling.Ignore,
            //    DefaultValueHandling = DefaultValueHandling.Include,
            //    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            //};

            // JsonFormatter.SerializerSettings are initially set up in Runnymede.Website.WebApiConfig.CustomizeConfiguration
            return GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
        }

        public static string SerializeAsJson(object value)
        {
            return JsonConvert.SerializeObject(value, GetGlobalJsonSettings());
        }

        public static T DeserializeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetGlobalJsonSettings());
        }

        /// <summary>
        /// Uses a custom Json.NET serializer to turn .NET objects into JavaScript literal representation.
        /// Note that the output is not valid JSON because the property names aren't wrapped in quotes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SerializeAsJavaScript(object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                //var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var serializer = JsonSerializer.Create(GetGlobalJsonSettings());
                jsonWriter.QuoteName = false; // We don't want quotes around object names
                serializer.Serialize(jsonWriter, value);
                return stringWriter.ToString();
            }
        }

    }

    #region RawStringResult

    // IMPORTANT! We piggy-back OkNegotiatedContentResult because it is able to preserve HttpContext.Current between threads. ApiController.Ok<T>() internally uses OkNegotiatedContentResult.
    // Some other result classes, for example JsonResult(), fail to preserve the request context. A custom class using System.Net.Http.StringContent, ByteArrayContent etc. fails as well.
    // +https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Web.Http/Results/OkNegotiatedContentResult.cs   
    /// <summary>
    /// Use this class to return an already available JSON string from ApiController. Bypass extra processing by the built-in JSON formatter in the out-of-the-box result methods. Otherwise the string gets wrapped in double quotes.
    /// </summary>
    public class RawStringResult : OkNegotiatedContentResult<string>
    {

        public enum TextMediaType
        {
            PlainText,
            Json,
        }

        public RawStringResult(ApiController controller, string content, TextMediaType mediaType)
            : base(
            content,
            controller.Configuration.Services.GetContentNegotiator(),
            controller.Request,
            new[] { new RawStringFormatter(GetMediaTypeString(mediaType)) }
            )
        {
        }

        private static string GetMediaTypeString(TextMediaType mediaType)
        {
            return mediaType == TextMediaType.PlainText
                ? MediaType.PlainText
                : (mediaType == TextMediaType.Json
                 ? MediaType.Json // JsonMediaTypeFormatter.DefaultMediaType.MediaType // "application/json"
                 : null);
        }

        private class RawStringFormatter : MediaTypeFormatter
        {
            private Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            public RawStringFormatter(string mediaType)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
                SupportedEncodings.Add(encoding);
            }

            public override bool CanReadType(Type type)
            {
                return false;
            }

            public override bool CanWriteType(Type type)
            {
                return type == typeof(String);
            }

            public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
            {
                if (value != null)
                {
                    using (var writer = new StreamWriter(writeStream, encoding))
                    {
                        //writer.WriteLine(value.ToString());
                        writer.Write(value.ToString());
                    }
                }
                return Task.FromResult<object>(null);
            }
        }
    }

    #endregion




}