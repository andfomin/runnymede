using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Runnymede.Website.Utils
{
    public class ResourcesUtils
    {

        // We load the page from the provided url. Then extract the VideoId from the page itself.
        public static string ExtractVideoIdFromYoutubeUrl(string url)
        {
            //var url = "+http://youtu.be/DflLwLnztv4";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            var response = (HttpWebResponse)request.GetResponse();
            string html;
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                html = reader.ReadToEnd();
            }
            // YouTube includes metadata in the Microdata format on their pages.
            string id = null;
            var i1 = html.IndexOf("itemtype=\"http://schema.org/VideoObject\"");
            if (i1 > 0)
            {
                var i2 = html.IndexOf("itemprop=\"videoId\"", i1);
                if (i2 > 0)
                {
                    var i3 = html.IndexOf("content=\"", i2);
                    if (i3 > 0)
                    {
                        var i4 = html.IndexOf("\"", i3 + 10);
                        if (i4 > 0)
                        {
                            id = html.Substring(i3 + 9, i4 - i3 - 9);
                        }
                    }
                }
            }

            return url.Contains(id) ? id : null;
        }



    }
}