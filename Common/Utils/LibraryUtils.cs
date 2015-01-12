using Runnymede.Common.Models;
//using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Common.Utils
{
    public class LibraryUtils
    {
        // Corresponds to dbo.appTypes (FR....) and app.library.Formats (utils-library-data.ts)
        public static class Formats
        {
            public const string Html = "FRHTML";
            public const string Youtube = "FRYTVD";
            public const string CoreInventoryExponents = "FRCIEX";
        }

        // Corresponds to dbo.libCategories
        public static class KnownCategories
        {
            public const string GameCopycat = "APP1";
        }

        public static class KnownTags
        {
            public const string GameCopycat = "gamecopycat";
        }

        private const string YoutubeUrlBase = "https://www.youtube.com/watch?v=";

        private static bool IsYoutube(Uri uri)
        {
            return uri.Host == "www.youtube.com";
        }

        /// <summary>
        /// Try to load a page/whatever from the provided URL. The URL in the returned resource is after possible redirects, it may be different than the one supplyed by the user. 
        /// If it is a known host, e.g. www.youtube.com, we try to extract the metadata.
        /// </summary>
        /// <param name="proposedUrl">We get the proposedUrl from the wild web. Be cautious.</param>
        /// <returns>A partially filled ResourceDto object if validated successfully, otherwise null</returns>
        public static async Task<ResourceDto> TryValidateResource(string proposedUrl)
        {
            // We got the proposedUrl from the wild web. Be cautious.
            ResourceDto resource = null;
            string youtubeHtml = null;

            var uri = new UriBuilder(proposedUrl).Uri; // If uri does not specify a scheme, the scheme defaults to "http:".
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException();
            }

            var handler = new HttpClientHandler
            {
                UseCookies = false,
            };

            using (var client = new HttpClient(handler))
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var completionOption = IsYoutube(uri) ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;

                    var response = await client.GetAsync(uri, completionOption);

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the last Uri after possible redirects. AllowAutoRedirect is set to true in WebClient instances. For example, we can start from +http://youtu.be/DflLwLnztv4
                        uri = response.RequestMessage.RequestUri;
                        resource = new ResourceDto { NaturalKey = uri.ToString() };
                        var contentType = response.Content.Headers.ContentType.MediaType;
                        if (contentType.Contains("text/html"))
                        {
                            resource.Format = Formats.Html;
                        }

                        if (IsYoutube(uri))
                        {
                            // A link may point directly to the SWF for loading the embedded player. For example, +https://www.youtube.com/v/BmOpD46eZoA?start=36&end=65
                            var directSwfLink = contentType.Contains("application/x-shockwave-flash");
                            if (directSwfLink)
                            {
                                var path = uri.AbsolutePath;
                                if (path.StartsWith("/v/"))
                                {
                                    uri = new Uri(YoutubeUrlBase + path.Substring(3));
                                }
                            }

                            if ((completionOption == HttpCompletionOption.ResponseContentRead) && !directSwfLink)
                            {
                                youtubeHtml = await response.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                response.Dispose();
                                // Second request
                                youtubeHtml = await client.GetStringAsync(uri);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Wrong web address. " + ex.Message);
                }
            }

            // Continue for YouTube. Read the VideoId and the Title from the page.
            if (!String.IsNullOrEmpty(youtubeHtml))
            {
                string videoId = null;
                string title = null;

                //Get VideoId. YouTube includes metadata in the Microdata format on their pages.
                var i1 = youtubeHtml.IndexOf("itemtype=\"http://schema.org/VideoObject\"");
                if (i1 > 0)
                {
                    var i2 = youtubeHtml.IndexOf("itemprop=\"videoId\"", i1);
                    if (i2 > 0)
                    {
                        var i3 = youtubeHtml.IndexOf("content=\"", i2);
                        if (i3 > 0)
                        {
                            var i4 = youtubeHtml.IndexOf("\"", i3 + 10);
                            if (i4 > 0)
                            {
                                videoId = youtubeHtml.Substring(i3 + 9, i4 - i3 - 9);
                            }
                        }
                    }
                }

                // Get the original title of the video. "og:" is Facebook's Open Graph format
                var i11 = youtubeHtml.IndexOf("<meta property=\"og:title\" content=\"");
                if (i11 > 0)
                {
                    var i12 = youtubeHtml.IndexOf("\">", i11);
                    if (i12 > 0)
                    {
                        title = HttpUtility.HtmlDecode(youtubeHtml.Substring(i11 + 35, i12 - i11 - 35));
                    }
                }

                if (String.IsNullOrEmpty(videoId) || String.IsNullOrEmpty(title))
                {
                    throw new Exception("Unsupported format of YouTube page.");
                }

                // We are going to play the YouTube video in an embedded player, and we will pass the VideoId to it directly. Store the VideoId.
                resource.Format = Formats.Youtube;
                resource.NaturalKey = videoId;
                resource.Title = title;
                resource.HasVideo = true;
            }

            return resource;
        }


    }
}