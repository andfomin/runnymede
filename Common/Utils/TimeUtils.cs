using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.Common.Utils
{
    public static class TimeUtils
    {
        private const string AmbiguousTimezoneOffset = "Timezone offset is ambiguous.";
        public static int[] LocalTimeLowerLimits = new int[] { 2014, 1, 1, 0, 0, 0 };
        public static int[] LocalTimeUpperLimits = new int[] { 2099, 12, 31, 23, 59, 59 };

        /// <summary>
        /// Throws exception if localTime does not correspond to localTimezoneOffset.
        /// </summary>
        /// <param name="localTime">Comes from client. See app.getLocalTimeInfo()</param>
        /// <param name="localTimezoneOffset">Comes from client. See app.getLocalTimeInfo()</param>
        public static void ThrowIfClientTimeIsAmbiguous(string localTime, int? localTimezoneOffset)
        {
            if (!ClientTimeIsOk(localTime, localTimezoneOffset))
            {
                throw new ArgumentException(AmbiguousTimezoneOffset);
            }
        }

        private static bool ClientTimeIsOk(string localTime, int? localTimezoneOffset)
        {
            var inferredTimeOffsetMin = InferTimeOffsetMin(localTime, localTimezoneOffset);
            // The inferred offset has the sign which is opposite to the sign of the incoming offset. See the comments in LoggingUtils.InferTimeOffsetMin()
            return (localTimezoneOffset + inferredTimeOffsetMin == 0);
        }

        private static int? CalculateTimeOffsetSec(string localTime)
        {
            if (!String.IsNullOrWhiteSpace(localTime))
            {
                // We get the localTime value from the wild web. Be cautious.
                var chunks = localTime.Split('/');
                //int dummy;
                //if (chunks.Count() == 6 && chunks.All(i => Int32.TryParse(i, out dummy)))
                //{
                //    var arr = chunks.Select(i => Convert.ToInt32(i)).ToArray();
                //    var clientTime = new DateTime(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], DateTimeKind.Utc);
                //    var span = clientTime - DateTime.UtcNow;
                //    var timeOffsetSec = Convert.ToInt32(span.TotalSeconds);
                //    return timeOffsetSec;
                //}
                if (chunks.Count() == 6)
                {
                    var parts = chunks
                        .Select(i =>
                        {
                            int part;
                            if (!Int32.TryParse(i, out part))
                            {
                                part = -1;
                            }
                            return part;
                        })
                        .ToArray();

                    var valid = parts
                        .Select((i, idx) => ((i >= LocalTimeLowerLimits[idx]) && (i <= LocalTimeUpperLimits[idx])))
                        .All(i => i);

                    if (valid)
                    {
                        var clientTime = new DateTime(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], DateTimeKind.Utc);
                        var span = clientTime - DateTime.UtcNow;
                        var timeOffsetSec = Convert.ToInt32(span.TotalSeconds);
                        return timeOffsetSec;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Infers the user's timezone offset based on the time reported by the browser. We do not need to use this function directly. If ThrowIfClientTimeIsAmbiguous() has passed well, use the supplyed localTimezoneOffset.
        /// </summary>
        /// <param name="localTime"></param>
        /// <param name="reportedTimezoneOffsetMin"></param>
        /// <returns></returns>
        private static int? InferTimeOffsetMin(string localTime, int? reportedTimezoneOffsetMin)
        {
            int? inferredOffsetMin = reportedTimezoneOffsetMin;

            var calculatedTimeOffsetSec = CalculateTimeOffsetSec(localTime);

            if (calculatedTimeOffsetSec.HasValue)
            {
                var fractualQuartersOfHour = (calculatedTimeOffsetSec.Value / 60.0 / 15.0);
                var roundedQuartersOfHour = Convert.ToInt32(Math.Round(fractualQuartersOfHour));
                inferredOffsetMin = (roundedQuartersOfHour * 15);
            }

            return inferredOffsetMin % (60 * 24); // Compensate if the client has set a wrong date.

            /* The outgoing offset has the sign which is opposite to the sign of the incoming offset. 
             * It is reported by JavaScript that way. It is discussed at +http://stackoverflow.com/a/21105733
             * Quote: Perhaps it's because when you see an offset in an ISO 8601 or RFC 822 string, that offset has already been applied. But when you call getTimezoneOffset() it's the offset to apply to bring it back to UTC.
             */
            /* Wikipedia  +http://en.wikipedia.org/wiki/Time_offset
             * The UTC offset is the difference in hours and minutes from UTC for a particular place and date. 
             */
            /* The JavaScript spec +http://ecma262-5.com/ELS5_HTML.htm#Section_15.9.5.26 
             * Date.prototype.getTimezoneOffset(). Returns the difference between local time and UTC time in minutes.
             */
            /* MDN +https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/getTimezoneOffset
             * The time-zone offset is the difference, in minutes, between UTC and local time.
             */
        }

        /// <summary>
        /// JavaScript uses milliseconds since the Unix epoch. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int64 GetMillisecondsSinceEpoch(DateTime value)
        {
            // // Calculate the difference on the client between the local time and provided time. It is used to show a warning on the page if the local clock is wrong.
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTimeKind must be Utc");
            }
            var timeSpan = value - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var millisecondsSinceEpoch = timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
            return millisecondsSinceEpoch;
        }

        /// <summary>
        /// LocalTime is a custom format. Its main purpose is to send timezone-neutral local time from client to server in an unambiguous format. 
        /// This method corresponds to app.encodeLocalTime() in utils.ts. We use this format with library history records to keep local time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string EncodeLocalTime(this DateTime time)
        {
            return time.ToString(@"yyyy\/M\/d\/h\/m\/s");
        }

    }
}
