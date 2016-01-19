using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class GamePickapicEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // The parameterless constructor is needed for deserialization by the Azure client.
        public GamePickapicEntity()
            : base()
        {
        }

        public GamePickapicEntity(string query, string json)
            : base(query, String.Empty)
        {
            Json = json;
        }

        public string Json { get; set; } // Max 64kB
    }

    // We use the entity class as a DTO as well to avoid redundand projection.
    [JsonObject]
    public class GameCopycatEntity2 : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey = KeyUtils.IntToKey(resourceId),
        // RowKey = KeyUtils.DateTimeToDescKey(DateTime.UtcNow),
        public string Transcript { get; set; } // Max 64kB
        public int UserId { get; set; }
        public string UserDisplayName { get; set; }

        //// For JSON serialization only. [JsonProperty("resourceId")] on the new RowKey makes the RowKey property serializable as well, so we end up with two properties sent over the wire.
        //[JsonProperty("resourceId")]
        //private string ResourceId
        //{
        //    get { return base.RowKey; }
        //}

        #region DisableSerializationOfAzureTableProperties
        /* JSON.net have a feature to dynamically hiding properties. It works by implemention a public method called ShouldSerialize[PropertyName] */

        public new string PartitionKey
        {
            get { return base.PartitionKey; }
            set { base.PartitionKey = value; }
        }

        public bool ShouldSerializePartitionKey()
        {
            return false;
        }

        public new string RowKey
        {
            get { return base.RowKey; }
            set { base.RowKey = value; }
        }

        public bool ShouldSerializeRowKey()
        {
            return false;
        }

        public new DateTimeOffset Timestamp
        {
            get { return base.Timestamp; }
            set { base.Timestamp = value; }
        }

        public bool ShouldSerializeTimestamp()
        {
            return false;
        }

        public new string ETag
        {
            get { return base.ETag; }
            set { base.ETag = value; }
        }

        public bool ShouldSerializeETag()
        {
            return false;
        }

        #endregion
    }

    public class SegmentDto
    {
        public int Start { get; set; }
        public int Finish { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
        public DateTime Time { get; set; }
    }

  public  class LuckyDigits
    {
        public DateTime Date { get; set; }
        public string Digits { get; set; } // CSV
    }

}