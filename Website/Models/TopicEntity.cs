using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class TopicEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public TopicEntity()
        {
        }

        // PartitionKey = string Math.Abs((title + lines).GetHashCode()).ToString("D10"); // If required, the number is pre-padded with zeros.
        // RowKey = string Title
        public string Type { get; set; }
        public string Lines { get; set; }
    }
}