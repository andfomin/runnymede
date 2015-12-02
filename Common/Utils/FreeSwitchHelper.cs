using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.Common.Utils
{
    public class FreeSwitchHelper
    {
        private static Lazy<HttpClient> lazyHttpClient = new Lazy<HttpClient>(InitHttpClient, true);

        private HttpClient httpClient
        {
            get
            {
                // HttpClient is not created until we access the Value property of the lazy initializer. Lazy<T> initialization is thread-safe.
                return lazyHttpClient.Value;
            }
        }

        // Initialize synchronously.
        private static HttpClient InitHttpClient()
        {
            var baseAddress = ConfigurationManager.AppSettings["Freeswitch.BaseAddress"];
            var username = ConfigurationManager.AppSettings["Freeswitch.Username"];
            var password = ConfigurationManager.AppSettings["Freeswitch.Password"];

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
            };

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            return httpClient;
        }

        public async Task<string> SendRpc(string path, string successfulResponseStartsWith = null)
        {
            var response = await this.httpClient.GetStringAsync(path);

            if (!String.IsNullOrEmpty(successfulResponseStartsWith) && !response.StartsWith(successfulResponseStartsWith))
            {
                throw new Exception(response);
            }

            return response;
        }



    }
}
