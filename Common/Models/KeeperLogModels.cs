using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Common.Models
{

    public class KeeperLogEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey = string "ObservedTime"
        // RowKey = empty string  
        public string LogData { get; set; }
    }

    public class KeeperLogData
    {
        public LoggingUtils.Kind Kind { get; set; }
        public DateTime Time { get; set; }
        public string ExtId { get; set; }
        public string RefererUrl { get; set; }
        public string Host { get; set; }
        public string UserAgent { get; set; }
        public IEnumerable<string> Languages { get; set; }
    }

}