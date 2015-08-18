using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class PayPalLogEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public enum NotificationKind
        {
            PDT, // Payment Data Transfer
            IPN,
            IPNResponse,
            Email,
            DetailsRequest,
            Error,
            PostingToDatabase
        }

        public PayPalLogEntity()
        {
        }

        // PartitionKey = string "Tx"
        // RowKey = string "ObservedTimeUnique"
        public string Kind { get; set; } // NotificationKind .ToString()
        public string LogData { get; set; }


    }
}