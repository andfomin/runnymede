using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Website.Utils
{
    public static class LoggingUtils
    {
        public enum Kind
        {
            Referer,
            Signup,
            Login
        }

        public const string KeeperCookieName = "keeper";
        public const string AmbiguousTimezoneOffset = "Timezone offset is ambiguous.";

        private static long SequenceCounter = 0;

        public static string GetUniquifiedObservedTime()
        {
            Interlocked.Increment(ref SequenceCounter);
            var now = DateTime.UtcNow;
            return now.ToString("u") + now.Millisecond.ToString("D3") + (SequenceCounter % 100000).ToString("D5"); // If required, the number is pre-padded with zeros.        
        }

        public static string GetUniqueObservedTime()
        {
            return DateTime.UtcNow.ToString("u") + " " + Guid.NewGuid().ToString("N").ToUpper();
        }

        private static ITableEntity CreateKeeperLogEntity(string logData)
        {
            //Interlocked.Increment(ref SequenceCounter);
            var now = DateTime.UtcNow;
            var entity = new KeeperLogEntity
            {
                PartitionKey = now.ToString("u"), // yyyy-MM-dd HH:mm:ssZ
                RowKey = Guid.NewGuid().ToString("N").ToUpper(),
                //RowKey =  now.Millisecond.ToString("D3") + (SequenceCounter % 100000).ToString("D5"), // If required, the number is pre-padded with zeros.
                LogData = logData,
            };
            return entity;
        }

        public static void WriteKeeperLog(string logData)
        {
            AzureStorageUtils.InsertEntry(AzureStorageUtils.KeeperLogTableName, CreateKeeperLogEntity(logData));
        }

        public static async Task WriteKeeperLogAsync(string logData)
        {
            await AzureStorageUtils.InsertEntryAsync(AzureStorageUtils.KeeperLogTableName, CreateKeeperLogEntity(logData));
        }

        public static int? CalculateTimeOffsetSec(string localTime)
        {
            if (!string.IsNullOrWhiteSpace(localTime))
            {
                // We get the localTime value from the wild web. Be cautious.
                try
                {
                    var parts = localTime.Split('/');
                    int dummy;
                    if (parts.Count() == 6 && parts.All(i => Int32.TryParse(i, out dummy)))
                    {
                        var arr = parts.Select(i => Convert.ToInt32(i)).ToArray();
                        var clientTime = new DateTime(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], DateTimeKind.Utc);
                        var span = clientTime - DateTime.UtcNow;
                        var timeOffsetSec = Convert.ToInt32(span.TotalSeconds);
                        return timeOffsetSec;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Infers the user's timezone offset based on the time reported by the browser.
        /// </summary>
        /// <param name="localTime"></param>
        /// <param name="reportedTimezoneOffsetMin"></param>
        /// <returns></returns>
        public static int? InferTimeOffsetMin(string localTime, int? reportedTimezoneOffsetMin)
        {
            int? inferredOffsetMin = reportedTimezoneOffsetMin;

            var calculatedTimeOffsetSec = CalculateTimeOffsetSec(localTime);

            if (calculatedTimeOffsetSec.HasValue)
            {
                var fractualQuartersOfHour = (calculatedTimeOffsetSec.Value / 60.0 / 15.0);
                var roundedQuartersOfHour = Convert.ToInt32(Math.Round(fractualQuartersOfHour));
                inferredOffsetMin = (roundedQuartersOfHour * 15);
            }

            return inferredOffsetMin % (60 * 24); // Some clients have set a wrong date.

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

        public static bool ClientTimeIsOk(string localTime, int? localTimezoneOffset)
        {
            var inferredTimeOffsetMin = InferTimeOffsetMin(localTime, localTimezoneOffset);
            // The inferred offset has the sign which is opposite to the sign of the incoming offset. See the comments in LoggingUtils.InferTimeOffsetMin()
            return (localTimezoneOffset + inferredTimeOffsetMin == 0);
        }




    }
}