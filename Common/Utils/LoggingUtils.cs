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

        private static ITableEntity CreateKeeperLogEntity(string logData)
        {
            var entity = new KeeperLogEntity
            {
                PartitionKey = KeyUtils.GetCurrentTimeKey(),
                RowKey = String.Empty,
                LogData = logData,
            };
            return entity;
        }

        public static async Task WriteKeeperLogAsync(string logData)
        {
            await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.KeeperLog, CreateKeeperLogEntity(logData));
        }
    }



}