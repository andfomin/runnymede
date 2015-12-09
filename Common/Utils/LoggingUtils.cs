using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Common.Utils;
using Runnymede.Common.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Runnymede.Common.Utils
{
    public static class LoggingUtils
    {
        public enum Kind
        {
            Referer,
            Signup,
            Login
        }

        public static async Task WriteKeeperLogAsync(string logData)
        {
            var entity = new KeeperLogEntity
            {
                PartitionKey = KeyUtils.GetCurrentTimeKey(),
                RowKey = String.Empty,
                LogData = logData,
            };

            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.KeeperLog, entity);
        }
    }



}