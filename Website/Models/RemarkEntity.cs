using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class RemarkEntity: Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public RemarkEntity()
        {
        }

        // string PartitionKey = int ReviewId // Originally int formatted as ToString("D10") using AzureStorageUtils.IntToKey
        // string RowKey = string Id // Six Base36 digits pre-padded with zeros.
        public int Start { get; set; }
        public int Finish { get; set; }
        public string Tags { get; set; } // May hold many tags separated by comma.
        public string Text { get; set; }
        public bool Starred { get; set; }
    }
}