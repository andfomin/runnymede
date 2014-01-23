using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class StatisticsEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public StatisticsEntity()
        {
        }

        // PartitionKey = string Exercise.UserId
        // RowKey = string CreateTime //  "yyyy-MM-dd HH:mm:ssZ" . Formatted using AzureStorageUtils.DateTimeToKey() 
        public int ExerciseId { get; set; }
        public int ReviewId { get; set; }
        public int? ExerciseLength { get; set; }
        public int TotalRemarkCount { get; set; }
        public int TotalTagCount { get; set; }
        public int UntaggedRemarkCount { get; set; }
        public string TagCounts { get; set; }

        // To serialize/deserialize by Json.Net
        public class TagCount
        {
            public TagCount(string t, int c)
            {
                this.T = t;
                this.C = c;
            }

            public string T { get; private set; } // Tag
            public int C { get; private set; } // Count
        }
    }
}