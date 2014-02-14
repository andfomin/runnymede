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

        public static int? InferTimeOffsetMin(string localTime, int? reportedTimezoneOffsetMin)
        {
            int? inferredOffsetMin = reportedTimezoneOffsetMin;

            var calculatedTimeOffsetSec = CalculateTimeOffsetSec(localTime);

            if (calculatedTimeOffsetSec.HasValue)
            {
                var fractualQuartersOfHour = (calculatedTimeOffsetSec.Value / 60.0 / 15.0);
                var roundedQuartersOfHour = Convert.ToInt32(Math.Round(fractualQuartersOfHour));
                inferredOffsetMin = roundedQuartersOfHour * 15;
            }

            return inferredOffsetMin;
        }




    }
}