using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Runnymede.Common.Utils
{

    public static class MediaType
    {
        public const string Mpeg = "audio/mpeg"; // The canonical type for MP3
        public const string Mp3 = "audio/mp3"; // Alias introduced by Chrome
        public const string Amr = "audio/amr";
        public const string Gpp = "audio/3gpp";
        public const string QuickTime = "video/quicktime";
        public const string Jpeg = MediaTypeNames.Image.Jpeg; // "image/jpeg";
        public const string Octet = MediaTypeNames.Application.Octet;// "application/octet-stream";
        public const string Json = "application/json"; // JsonMediaTypeFormatter.DefaultMediaType.MediaType
        public const string PlainText = MediaTypeNames.Text.Plain; // "text/plain"
        public const string Xml = MediaTypeNames.Text.Xml; // "text/xml" // +http://www.ietf.org/rfc/rfc7303.txt Section 9.2: "information for text/xml is in all respects the same as that given for application/xml"

        public static string GetExtension(string contentType)
        {
            switch (contentType)
            {
                case Mpeg:
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
            Xml
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
            switch (mediaType)
            {
                case TextMediaType.PlainText:
                    return MediaType.PlainText;
                case TextMediaType.Json:
                    return MediaType.Json;
                case TextMediaType.Xml:
                    return MediaType.Xml;
                default:
                    return null;
            }
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

    #region Skip32 32-bit block cipher
    // 32-bit block cipher. +https://github.com/eleven41/Eleven41.Skip32
    public static class Skip32Utils
    {
        /// <summary>
        /// Encript an int value into a 8-char hex string like "FFFFFFFF"
        /// </summary>
        /// <param name="value">Original value</param>
        /// <param name="skip32Key">The key must be 10 chars long</param>
        /// <returns>8-char hex string like "FFFFFFFF"</returns>
        public static string EncriptIntToHexString(int value, string skip32Key)
        {
            if (skip32Key.Length != 10)
            {
                throw new ArgumentOutOfRangeException("The key must be 10 chars long.");
            }
            var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
            var encrypted = cipher.Encrypt(value);
            // Convert to UInt32 to avoid a sign.
            var bytes = BitConverter.GetBytes(encrypted);
            var unsigned = BitConverter.ToUInt32(bytes, 0);
            var encryptedString = unsigned.ToString("X"); // 8 chars
            return encryptedString;
        }

        /// <summary>
        /// Decript value preveously encripted by EncriptIntToHexString()
        /// </summary>
        /// <param name="value">Hex string, like "FFFFFFFF"</param>
        /// <param name="skip32Key">The same key as was passed to EncriptIntToHexString()</param>
        /// <param name="defaultValue">If a non-null value is passed, fail silently and return it. Otherwise throw.</param>
        /// <returns>Original int value passed to EncriptIntToHexString()</returns>
        public static int DecriptHexStringToInt(string value, string skip32Key, int? defaultValue = null)
        {
            try
            {
                if (skip32Key.Length != 10)
                {
                    throw new ArgumentOutOfRangeException("The key must be 10 chars long.");
                }
                var unsigned = UInt32.Parse(value, NumberStyles.HexNumber);
                var bytes = BitConverter.GetBytes(unsigned);
                var encrypted = BitConverter.ToInt32(bytes, 0);
                var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
                return cipher.Decrypt(encrypted);
            }
            catch
            {
                if (defaultValue.HasValue)
                {
                    // Fail silently.
                    return defaultValue.Value;
                }
                else
                {
                    throw;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Use it for transferring information to the client side. It is used for custom error handling in WebApi. 
    /// </summary>
    public class UserAlertException : Exception
    {
        public UserAlertException(string message)
            : base(message)
        {
        }

        public UserAlertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // See Runnymede.Website.Utils.CustomExceptionResult
    }



}