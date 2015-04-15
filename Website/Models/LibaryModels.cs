using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class LibraryHistoryEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey = KeyUtils.IntToKey(userId)
        // RowKey = KeyUtils.LocalTimeToInvertedKey(localTime), We keep the local time and order last records first for retrieval. 
        public int ResourceId { get; set; }
    }
}