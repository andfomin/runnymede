using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Common.LibraryIndex
{
    public class AzureSearchHelper
    {
        public const int SearchResults_MaxLimit = 20;

        /* HttpClient and handlers are designed for reuse and thread safety. +http://wcf.codeplex.com/discussions/277850
         * +http://chimera.labs.oreilly.com/books/1234000001708/ch14.html#_multiple_instances
         * Although HttpClient does indirectly implement the IDisposable interface, the recommended usage of HttpClient is not to dispose of it after every request. ... 
         * HttpClient is a threadsafe class and can happily manage multiple parallel HTTP requests. 
         */
        private static readonly Lazy<HttpClient> lazyHttpClient = new Lazy<HttpClient>(InitHttpClient, true);
        private static Uri serviceUriSingleton;
        private static JsonSerializerSettings jsonSettingsSingleton;

        private static HttpClient InitHttpClient()
        {
            var apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            return httpClient;
        }

        private HttpClient httpClient
        {
            get
            {
                // HttpClient is not created until we access the Value property of the lazy initializer. Lazy<T> initialization is thread-safe.
                return lazyHttpClient.Value;
            }
        }
        private Uri serviceUri
        {
            get
            {
                if (serviceUriSingleton == null)
                {
                    var searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
                    serviceUriSingleton = new Uri("https://" + searchServiceName + ".search.windows.net");
                }
                return serviceUriSingleton;
            }
        }
        private JsonSerializerSettings jsonSettings
        {
            get
            {
                if (jsonSettingsSingleton == null)
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented, // for readability, change to None for compactness
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    };
                    jsonSettings.Converters.Add(new StringEnumConverter());
                    jsonSettingsSingleton = jsonSettings;
                }
                return jsonSettingsSingleton;
            }
        }

        private const string ApiVersion = "2014-07-31-Preview";
        private const string ApiVersionString = "api-version=2014-07-31-Preview";

        private string indexName;
        private int userId;

        /// <summary>
        /// Performs a request to the custom Azure Search index.
        /// </summary>
        /// <param name="indexKind">Determines the index to use, either Common or Personal.</param>
        /// <param name="userId">If the Personal index is used, the results are filtered by userId</param>
        //public AzureSearchHelper(LibraryIndexHelper.IndexKinds indexKind, int userId = 0)
        //{
        //    this.indexName = LibraryIndexHelper.GetIndexName(indexKind);
        //    this.userId = userId;
        //}

        public AzureSearchHelper(string indexName, int userId = 0)
        {
            this.indexName = indexName;
            this.userId = userId;
        }

        public AzureSearchHelper(LibraryIndexHelper.IndexKinds indexKind, int userId = 0)
            : this(LibraryIndexHelper.GetIndexName(indexKind), userId)
        {
        }

        // -------------- Searching --------------------
        #region Searching

        public async Task<string> Search(int offset, int limit, string categoryId, string searchText, bool useCount = true, string[] fields = null)
        {
            if (String.IsNullOrWhiteSpace(searchText) && String.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("Empty query.");
            }

            var queryParameters = new Dictionary<string, string>
            {
                // TODO : Implement support for @odata.nextLink
                // If you specify a value greater than 1000 and there are more than 1000 results, only the first 1000 results will be returned, along with a link to the next page of results (see @odata.nextLink).
                { "$count", useCount ? "true" : "false" },
                { "$skip", offset.ToString() },
                { "$top", limit.ToString() },
            };

            // Omitting the "search" parameter has the same effect as setting it to *.
            if (!String.IsNullOrWhiteSpace(searchText))
            {
                queryParameters.Add("search", searchText);
            }

            var filters = new List<string>();

            if (this.userId != 0)
            {
                filters.Add("userId eq " + userId.ToString());
            }

            if (!String.IsNullOrWhiteSpace(categoryId))
            {
                // Look for a whitespace in categoryId. //if (categoryId.Contains(' '))
                if (new Regex(@"\s").IsMatch(categoryId))
                {
                    throw new ArgumentException("Bad category " + categoryId);
                }
                filters.Add("categoryPathIds/any(c: c eq '" + categoryId + "')");
            }

            if (filters.Any())
            {
                queryParameters.Add("$filter", String.Join(" and ", filters));
            }

            if (fields != null)
            {
                var select = String.Join(",", fields);
                queryParameters.Add("$select", select);
            }

            return await SendSearchRequest(queryParameters);
        }

        //public async Task<string> GetPersonalCategories()
        //{
        //    var queryParameters = new Dictionary<string, string>
        //    {
        //        // TODO : Implement support for @odata.nextLink
        //        // If you specify a value greater than 1000 and there are more than 1000 results, only the first 1000 results will be returned, along with a link to the next page of results (see @odata.nextLink).
        //        { "$top", "1000" },
        //        { "$filter", "userId eq " + this.userId.ToString() },
        //        { "$select", "categoryPathIds" },
        //    };
        //    return await SendSearchRequest(queryParameters);
        //}

        private string EscapeODataString(string s)
        {
            return Uri.EscapeDataString(s).Replace("\'", "\'\'");
        }

        public async Task<string> SendSearchRequest(Dictionary<string, string> queryParameters)
        {
            /* Do not use Dictionary, if you use "$facet". Dictionary replaces an old value with a new one if the keys are the same.
             * Whereas Azure Search API allows for many $facet parameters in a query string, for example "&facet=color&facet=categoryName&facet=listPrice,values:10|25|100|500|1000|2500"   
             * As a workaround, alias $facet keys in Dictionary, then replase the aliases in the final query string.
             */
            var query = HttpUtility.ParseQueryString(string.Empty); // returns System.Web.HttpValueCollection: System.Collections.Specialized.NameValueCollection
            foreach (var p in queryParameters)
            {
                query.Add(p.Key, p.Value);
            }
            query.Add("api-version", ApiVersion);
            // Carefully escape and combine input for filters, injection attacks that are typical in SQL also apply here. No "drop table" risk, but a well injected "or" can cause unwanted disclosure
            // HttpValueCollection.ToString() does UriQueryUtility.UrlEncode.
            var queryString = query.ToString();
            return await SendSearchRequest(queryString);
        }

        private async Task<string> SendSearchRequest(string queryString)
        {
            var uri = new Uri(this.serviceUri, "/indexes/" + this.indexName + "/docs?" + queryString);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await this.httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content == null ? null : response.Content.ReadAsStringAsync().Result;
                throw new Exception("Search request failed: " + error);
            }
            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        // --------------- Management -----------------
        #region Management

        private string SerializeJson(object value)
        {
            return JsonConvert.SerializeObject(value, jsonSettings);
        }

        //private T DeserializeJson<T>(string json)
        //{
        //    return JsonConvert.DeserializeObject<T>(json, jsonSettings);
        //}

        public async Task<HttpResponseMessage> SendIndexRequestAsync(HttpMethod method, Uri uri, string json = null, bool checkSuccess = true)
        {
            UriBuilder builder = new UriBuilder(uri);
            string separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : "&";
            builder.Query = builder.Query.TrimStart('?') + separator + ApiVersionString;

            var request = new HttpRequestMessage(method, builder.Uri);

            if (json != null)
            {
                // System.Net.Http.Formatting.JsonMediaTypeFormatter is in System.Net.Http.Formatting.dll in package Microsoft.AspNet.WebApi.Client.5.2.2 
                request.Content = new StringContent(json, Encoding.UTF8, /*JsonMediaTypeFormatter.DefaultMediaType.MediaType*/"application/json");
            }

            var response = await this.httpClient.SendAsync(request);
            if (checkSuccess)
            {
                response.EnsureSuccessStatusCode();
            }
            return response;
        }

        private bool IndexExists()
        {
            var uri = new Uri(this.serviceUri, String.Format("/indexes/{0}", this.indexName));
            var response = SendIndexRequestAsync(HttpMethod.Get, uri, null, false).Result;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }

        private void DeleteIndex()
        {
            var uri = new Uri(this.serviceUri, String.Format("/indexes/{0}", this.indexName));
            SendIndexRequestAsync(HttpMethod.Delete, uri).Wait();
        }

        private void CreateIndex(object indexDefinition)
        {
            var uri = new Uri(this.serviceUri, "/indexes");
            var json = SerializeJson(indexDefinition);
            SendIndexRequestAsync(HttpMethod.Post, uri, json).Wait();
        }

        public void RecreateIndex(object indexDefinition)
        {
            if (IndexExists())
            {
                DeleteIndex();
            }
            CreateIndex(indexDefinition);
        }

        public async Task<HttpResponseMessage> SendDocumentsAsync(object batch)
        {
            var uri = new Uri(serviceUri, String.Format("/indexes/{0}/docs/index", this.indexName));
            var json = SerializeJson(batch);
            return await SendIndexRequestAsync(HttpMethod.Post, uri, json);
        }

        #endregion

    }

}