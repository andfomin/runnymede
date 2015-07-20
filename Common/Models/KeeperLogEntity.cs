using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Common.Models
{
    public class KeeperLogEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        //public KeeperLogEntity()
        //{
        //}

        // PartitionKey = string "ObservedTime"
        // RowKey = string "Uniqueifier" 
        public string LogData { get; set; }
    }
}