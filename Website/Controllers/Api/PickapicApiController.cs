using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Runnymede.Website.Models;
using Runnymede.Common.Utils;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Runnymede.Website.Controllers.Api
{
    [RoutePrefix("api/pickapic")]
    public class PickapicApiController : ApiController
    {

        // GET: /api/pickapic/?q=qwerty
        [Route("")]
        public async Task<IHttpActionResult> GetCollection(string q)
        {
            if (String.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Empty query.");
            }
            var collectionName = GeneralUtils.SanitizeSpaceSeparatedWords(q);
            
            // Try to find the picture collection for the query in the internal Table storage.
            var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.GamePicapick);
            var operation = TableOperation.Retrieve<GamePickapicEntity>(collectionName, String.Empty);
            var entity = (await table.ExecuteAsync(operation)).Result as GamePickapicEntity;
            string json = entity != null ? entity.Json : null;

            // If the data not found in our internal Table, query the YouTube API.
            if (json == null)
            {
                // We do not use the Google API Client library to materialize result as a POCO. Anyway the API itself is RESTful, and JSON can be parsed easily. Avoid overhead, overkill, bloatware etc.
                // +https://developers.google.com/apis-explorer/#p/youtube/v3/youtube.search.list

                var youtubeParams = HttpUtility.ParseQueryString(String.Empty); // returns System.Web.HttpValueCollection: System.Collections.Specialized.NameValueCollection
                youtubeParams["key"] = ConfigurationManager.AppSettings["YoutubeApiKey"];
                youtubeParams["part"] = "snippet";
                youtubeParams["type"] = "video";
                youtubeParams["maxResults"] = "50";
                youtubeParams["q"] = collectionName;
                youtubeParams["fields"] = "items(id,snippet(thumbnails(high)))";
                var youtubeQueryString = youtubeParams.ToString();

                var url = "https://www.googleapis.com/youtube/v3/search?" + youtubeQueryString;

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                string response = null;
                using (var client = new HttpClient(handler))
                {
                    // If User-Agent is not sent, the server ignores "Accept-Encoding: gzip, deflate" and does not compress the response. The observed compression is 10kB -> 1kB.
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "google-api-dotnet-client/1.8.1.31685 (gzip)");

                    response = await client.GetStringAsync(url);
                }

                var urls = JObject.Parse(response)
                    .GetValue("items")
                    .Children()
                    .Select(i => i["snippet"]["thumbnails"]["high"]["url"].Value<string>())
                    .OrderBy(i => Guid.NewGuid()) // Shuffle
                    .ToArray();

                var controlNumber = Math.Abs(String.Join(String.Empty, urls).GetHashCode()) % 100;
                json = JsonUtils.SerializeAsJson(new { CollectionName = collectionName, ControlNumber = controlNumber, Urls = urls, });
                entity = new GamePickapicEntity(collectionName, json);
                await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.GamePicapick, entity);
            }

            return new RawStringResult(this, json, RawStringResult.TextMediaType.Json);
        }

    }
}
