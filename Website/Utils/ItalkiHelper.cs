using Itenso.TimePeriod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.Website.Utils
{

    public class ItalkiHelper
    {
        private const string ItalkiLoginUrl = "https://www.italki.com/login";
        private const string ItalkiDashbordUrl = "http://www.italki.com/dashboard";
        private const string ItalkiCloakUrl = "http://www.italki.com/teachers";
        private const string ItalkiWebServicesUrl = "https://secure.italki.com/WebServices/GetData.aspx";

        public static TimeSpan ItalkiTimeSlotDuration = TimeSpan.FromMinutes(30); // minutes. 
        private static TimeSpan ReloginInterval = TimeSpan.FromMinutes(10); // minutes

        public const string TimeSlotUnavailableError = "The time you are scheduling was taken by other learners. Please try another time.";

        /* HttpClient and handlers are designed for reuse and thread safety. +http://wcf.codeplex.com/discussions/277850
         * +http://chimera.labs.oreilly.com/books/1234000001708/ch14.html#_multiple_instances
         * Although HttpClient does indirectly implement the IDisposable interface, the recommended usage of HttpClient is not to dispose of it after every request. ... 
         * HttpClient is a threadsafe class and can happily manage multiple parallel HTTP requests. 
         */
        private static Lazy<HttpClient> lazyHttpClient = new Lazy<HttpClient>(InitHttpClient, true);

        private HttpClient httpClient
        {
            get
            {
                // HttpClient is not created until we access the Value property of the lazy initializer. Lazy<T> initialization is thread-safe.
                return lazyHttpClient.Value;
            }
        }

        private static DateTime LoginMoment;

        // Initialize synchronously.
        private static HttpClient InitHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var httpClient = new HttpClient(handler, true);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, */*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Log in
            var username = ConfigurationManager.AppSettings["Italki.Username1"];
            var password = ConfigurationManager.AppSettings["Italki.Password1"];

            // Get infected with cookies.
            httpClient.GetStringAsync(ItalkiLoginUrl).Wait();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("LOGIN_PA_USERNAME", username),
                new KeyValuePair<string, string>("LOGIN_PA_PASSWORD", password),
                new KeyValuePair<string, string>("cookie-is-long-term", "1"),
            });

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(ItalkiLoginUrl),
                Method = HttpMethod.Post,
                Content = content,
            };
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", ItalkiLoginUrl);

            var response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var success = json.Contains(@"""success"": 1");

            // Itaki suggests in the login response that we go to the dashboard page.
            // We go to the page to get another portion of cookies (????) ("ASP.NET_SessionId" is not set there). 
            // Actually we can get redirected 302 on first real requests regardless wether we went here.
            // It is most probably relaited to timing. This step may just add some delay for stabilizing some state??? It adds about 500 msec.
            var goToDashboard = json.Contains(ItalkiDashbordUrl);
            if (goToDashboard)
            {
                httpClient.GetStringAsync(ItalkiDashbordUrl).Wait();
            }

            // Find out when the authorization cookie expires. It is set for 90 days by Italki.            
            //var authCookie = handler.CookieContainer.GetCookies(request.RequestUri)[".italkiCookie$.italki.com"];
            //if (authCookie != null)
            //{
            //    var expires = authCookie.Expires;
            //    httpClient.GetStringAsync("http://www.example.com/?q=" + expires.ToString("u")); // Poor-man's debug console (aka Fiddler)
            //}

            LoginMoment = DateTime.UtcNow;

            Console.WriteLine("Login success: {0}", success);
            return httpClient;
        }

        public static void EnsureIsReady()
        {
#if !DEBUG
            // The singleton HttpClient is lazyly initialized. We log in here preventively if not done yet.
            var oldHttpClient = lazyHttpClient.Value;
            // The autorization cookie lasts for 90 days. We need to re-login periodically.
            if (DateTime.UtcNow > (LoginMoment + ReloginInterval))
            {
                lazyHttpClient = new Lazy<HttpClient>(InitHttpClient, true);
                var newHttpClient = lazyHttpClient.Value;
                oldHttpClient.CancelPendingRequests();
                oldHttpClient.Dispose();
            }
#endif
        }

        public async Task<string> LoadPage(string url)
        {
            try
            {
                return await this.httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if DEBUG
                throw;
#else
                throw new Exception("External service. GetStringAsync() failed");
#endif
            }
        }

        //public IEnumerable<UserTimeRange> ProcessPage(string html)
        //{
        //    var vacantSlots = FindVacantTimeSlots(html);
        //    var userVacantSlots = vacantSlots.Select(i => new UserTimeRange(ItalkiHelper.UserId, i.Start, i.End));
        //    return userVacantSlots;
        //}

        // Find vacant time slots by subtracting the occupyed slots from the template slots. 
        public IEnumerable<ITimePeriod> GetVacantPeriods(string html)
        {
            /* !!! Warning! If the balance on the account is low, the page does not have the schedule data, but shows payment offer.
             * After enabling error details in web.config, the error text is "Parsing the external service data. Snippet not found. 43 9"
             */

            // Save on text parsing. Pre-extract the script texts.
            var paramScriptText = FindText(html, "function ShowMsgBox(title,scr,width,height)", "</script>"); ;
            var listScriptText = FindText(html, @"<script type=""text/javascript"">var sUseList", "</script>"); ;

            EnsureTimeIsUTC(paramScriptText);
            var firstDay = ParseFirstDay(paramScriptText);
            //var timeTy = ParseTimeTy(paramScriptText);

            var timeArrList = ProcessTimeArrList(listScriptText, firstDay);
            var tDaysOfWeekList = ProcessTDaysOfWeekList(listScriptText, firstDay);
            timeArrList.AddAll(tDaysOfWeekList);

            // Replicate the next week. The length of timeArrList and tDaysOfWeekList is 8 items, so we will produce some overlapped duplicates. The duplicates will be eliminated by the combining operation.
            var nextEightDays = timeArrList
                .Select(i => new TimeRange(i.Start.AddDays(7), i.Duration))
                .ToList();
            timeArrList.AddAll(nextEightDays);

            CombinePeriods(timeArrList);

            var tUseList = ProcessTUseList(listScriptText, firstDay);
            //var notUseTime = ProcessNotUseTime(listScriptText, firstDay); // We impose the deadline ourselves.
            //tUseList.AddAll(notUseTime);

            var subtractor = new TimePeriodSubtractor<TimeRange>();
            var vacantPeriods = subtractor.SubtractPeriods(timeArrList, tUseList);

            // Do not shred long stretches into chunks because when they are processed down the stream they will be merged again.
            return vacantPeriods;
        }

        // Extract text from the HTML page
        private string FindText(string html, string startSnippet, string endSnippet)
        {
            var i1 = html.IndexOf(startSnippet);
            var i2 = html.IndexOf(endSnippet, i1 + 1);
            if ((i1 == -1) || (i2 == -1))
            {
                // Write the poison HTML page to the log.
                var partitionKey = KeyUtils.GetCurrentTimeKey();
                var entity = new ExternalSessionLogEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = "Error_FindText",
                    Data = html,
                };
                AzureStorageUtils.InsertEntity(AzureStorageUtils.TableNames.ExternalSessions, entity);

                throw new FormatException("Parsing the external service data. Snippet not found. " + startSnippet.Length.ToString() + " " + endSnippet.Length.ToString());
            }
            var shift = startSnippet.Length;
            return html.Substring(i1 + shift, i2 - i1 - shift);
        }

        // We must set up the timezone in the Italki user profile manually. Make sure the data is for UTC.
        private void EnsureTimeIsUTC(string html)
        {
            /* var TimezoneText='Jun 22, 2015 20:20 (UTC)Coordinated Universal Time';
             * var 
             */
            var snippet = FindText(html, "var TimezoneText='", "var ");
            if (snippet.IndexOf("(UTC)Coordinated Universal Time';") == -1)
            {
                throw new FormatException("Parsing the external service data. The base timezone is not UTC.");
            }
        }

        // Other values are relative to this base date.
        private DateTime ParseFirstDay(string html)
        {
            /* var stringtimes='2015-06-23'.split('-'); */
            var dateText = FindText(html, "var stringtimes='", "'.split('-');");
            DateTime date;
            var success = DateTime.TryParseExact(dateText, "yyyy-MM-dd", null,
                (System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal),
                out date);
            if (!success)
            {
                throw new FormatException("itaki schedule. The first day is unknown.");
            }
            return date;
        }

        // What it exactly means is unknown. It affects which original slots are disabled by the accupied slots. See the comment in ProcessTUseList().
        //private int ParseTimeTy(string html)
        //{
        //    /* var timety=1; */
        //    var intText = FindText(html, "var timety=", ";");
        //    return Int32.Parse(intText);
        //}

        // dayNumber is zero-based
        private DateTime TimeTextToDateTime(string timeText, int dayNumber, DateTime firstDay)
        {
            if (timeText == "24:00")
            {
                timeText = "00:00";
                dayNumber++;
            }
            var time = DateTime.ParseExact(timeText, "HH:mm", null);
            var timeOfDay = time.TimeOfDay;
            return firstDay.AddDays(dayNumber).Add(timeOfDay);
        }

        private ITimePeriodCollection CombinePeriods(ITimePeriodCollection periods)
        {
            var periodCombiner = new TimePeriodCombiner<TimeRange>();
            return periodCombiner.CombinePeriods(periods);
        }

        private ITimePeriodCollection ProcessTimeArrList(string html, DateTime firstDay)
        {
            var periods = new TimePeriodCollection();
            var arrText = FindText(html, "var timearrlist=", ";");
            var days = JArray.Parse(arrText);
            for (int i = 0; i < days.Count; i++)
            {
                var dayArr = (JArray)days[i];
                if (dayArr.Any())
                {
                    foreach (var timeArr in dayArr)
                    {
                        var start = TimeTextToDateTime((string)timeArr.First(), i, firstDay);
                        var end = TimeTextToDateTime((string)timeArr.Last(), i, firstDay);
                        var timeRange = new TimeRange(start, end);
                        periods.Add(timeRange);
                    }
                }
            }
            return CombinePeriods(periods);
        }

        // tDaysOfWeekList holds the initial weekly template for the schedule. The teacher indicates which slots are potentially available.
        private ITimePeriodCollection ProcessTDaysOfWeekList(string html, DateTime firstDay)
        {
            var periods = new TimePeriodCollection();
            var arrText = FindText(html, "var tDaysOfWeekList=", ";");
            var days = JArray.Parse(arrText);
            for (int i = 0; i < days.Count; i++)
            {
                var dayArr = (JArray)days[i];
                var duration = ItalkiTimeSlotDuration;
                foreach (var time in dayArr)
                {
                    var start = TimeTextToDateTime((string)time, i, firstDay);
                    var timeBlock = new TimeBlock(start, duration);
                    periods.Add(timeBlock);
                }
            }
            return CombinePeriods(periods);
        }

        // tUseList holds the taken time slots.
        private ITimePeriodCollection ProcessTUseList(string html, DateTime firstDay)
        {
            /* The italki code uses timeTy to make the time previous to a taken one unavilable. 
             * That reflects their schedule model. They work with start times, not with periods. So they use timeTy to secure the full length for the previous session.
             * That way they effectively filter out short vacant spaces, i.e. 30 minutes, since the minimal session length is 60 minutes.
            //var startSlot = slot >= timeTy ? slot - timeTy : slot;
             */
            var arrText = FindText(html, "var tUseList=", ";");
            return ProcessDayArray(arrText, firstDay);
        }

        // notUseTime holds disabled (grayed out) time slots. italki uses it to ensure that a session must be booked at least in 24 hours in advance.
        //private ITimePeriodCollection ProcessNotUseTime(string html, DateTime firstDay)
        //{
        //    var arrText = FindText(html, "var notUseTime=", ";");
        //    // I am not sure how many days may come in notUseTime, i.e. whether it is one- or two-dimentional. We force it to be 2D.
        //    if (!arrText.StartsWith("[[") && !arrText.EndsWith("]]"))
        //    {
        //        arrText = "[" + arrText + "]";
        //    }
        //    return ProcessDayArray(arrText, firstDay);
        //}

        private ITimePeriodCollection ProcessDayArray(string arrText, DateTime firstDay)
        {
            var periods = new TimePeriodCollection();
            var days = JArray.Parse(arrText);
            foreach (var day in days)
            {
                // The first item is the day number. The following items are half-hour times.
                var dayNumber = (int)day.First();
                var slots = day
                    .Skip(1)
                    .Select(i =>
                    {
                        var slot = (int)i;
                        var start = firstDay.AddDays(dayNumber).AddMinutes(slot * ItalkiTimeSlotDuration.TotalMinutes);
                        var end = start + ItalkiTimeSlotDuration;
                        return new TimeRange(start, end);
                    });
                periods.AddAll(slots);
            }
            return CombinePeriods(periods);
        }

        public async Task<long?> BookSession(int courseId, DateTime start, string referer, string tablePartitionKey)
        {
            // Time slot is encoded in a weired way.
            var origin = DateTime.UtcNow.Date;
            var diff = start - origin;
            var day = Convert.ToInt32(Math.Floor(diff.TotalDays)) - 1; // Day number for the next day is 0.
            if (day < 0)
            {
                throw new Exception(ItalkiHelper.TimeSlotUnavailableError);
            }
            var halfHour = (diff.Hours * 2) + (diff.Minutes == 30 ? 1 : 0); // Convert.ToInt32(1.0 * diff.Minutes / 30); // We expect minutes to be either 00 or 30.
            var timeText = String.Format("{0},{1}", day, halfHour);

            // We will write to the log.
            var partitionKey0 = KeyUtils.GetCurrentTimeKey();

            // Pre-booking validation. It might be redundand. We may want to do it just in case.
            // Note that the validation call does not send the courseId. That means the server uses a session(???) or a cookie or the Referer header. It may cause problems in a multi-teacher scenario, until we use a separate HttpClient for each teacher.
            //var content1 = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("MethodName", "ValidSessionTime"),
            //    new KeyValuePair<string, string>("time", timeText),
            //});
            //var request1 = new HttpRequestMessage
            //{
            //    RequestUri = new Uri(ItalkiWebServicesUrl),
            //    Method = HttpMethod.Post,
            //    Content = content1,
            //};
            //request1.Headers.Add("Referer", referer);

            //var response1 = await httpClient.SendAsync(request1);

            //var statusCode1 = (int)response1.StatusCode;
            //string responseContent1 = null;
            //if (response1.IsSuccessStatusCode)
            //{
            //    responseContent1 = await response1.Content.ReadAsStringAsync();
            //}

            //var entity1 = new ExternalSessionLogEntity
            //{
            //    PartitionKey = partitionKey,
            //    RowKey = "ValidationResponse",
            //    Data = responseContent1,
            //    HttpStatus = statusCode1,
            //};
            //await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.ExternalSessions, entity1);

            //response1.EnsureSuccessStatusCode();

            // Send the booking request
#if DEBUG
            var password = "12345678";
#else
            var password = ConfigurationManager.AppSettings["Italki.Password1"];
#endif
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("MethodName", "RequestSession"),
                new KeyValuePair<string, string>("time", timeText), // It will be Url-encoded by the form oject.
                new KeyValuePair<string, string>("tool", "1"),
                new KeyValuePair<string, string>("sim", "andrey.fomin3"),
                new KeyValuePair<string, string>("msg", ":)"), // ":-)  :)  :D  :o)  :]  :3  :c)  :>  =]  8)  =)  :}  :^)"
                new KeyValuePair<string, string>("pwd", password),
                new KeyValuePair<string, string>("cid", courseId.ToString()),
            });

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(ItalkiWebServicesUrl),
                Method = HttpMethod.Post,
                Content = content,
            };
            request.Headers.Add("Referer", referer);

            // Write to the log
            var entity = new ExternalSessionLogEntity
            {
                PartitionKey = tablePartitionKey,
                RowKey = "ItalkiRequest",
                Data = JsonUtils.SerializeAsJson(new
                {
                    CourseId = courseId,
                    Start = start,
                    Time = timeText,
                }),
            };
            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.ExternalSessions, entity);

            // Book the session with Italki
            var response = await httpClient.SendAsync(request);

            var statusCode = (int)response.StatusCode;
            string responseContent = null;
            if (response.IsSuccessStatusCode)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            // Write the response to the log
            entity = new ExternalSessionLogEntity
            {
                PartitionKey = tablePartitionKey,
                RowKey = "ItalkiResponse",
                Data = responseContent,
                HttpStatus = statusCode,
            };
            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.ExternalSessions, entity);

            response.EnsureSuccessStatusCode();

            long? extSessionId = null;
            try
            {
                var items = JArray.Parse(responseContent);
                /* +https://secure.italki.com/sessions/schedule_20131106.js?v=150302 , Line 853
                 * if(result[0]=="-2") $get("MsgPrompt").innerHTML=TS312;
                 * if(result[0]=="-1") $get("MsgPrompt").innerHTML=TS313;
                 * if(result[0]=="1") $get("MsgPrompt").innerHTML=TS314;
                 * if(result[0]=="-3") $get("MsgPrompt").innerHTML=TS315;
                 * In HTML
                 * var TS312="The time you\'re scheduling was taken by other students. Please try other time.";
                 * var TS313="Your payment account balance is not enough to pay for the session(s) you are scheduling.";
                 * var TS314="Trial session is not available.";
                 * var TS315="Incorrect password";
                 */
                var result = (string)items.FirstOrDefault();
                if ((result == "Y") || (result == "Y1"))
                {
                    var idArr = items[3];
                    if (idArr is JArray)
                    {
                        extSessionId = (long)idArr.First();
                    }
                }
            }
            // We may get an HTML instead of JSON in response.
            catch (JsonReaderException)
            {
            }

            return extSessionId;
        }

    }
}
