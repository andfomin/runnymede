using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Website.Utils
{
    public class AzureStorageUtils
    {
        public const string StorageConnectionStringName = "StorageConnection";
        public const string RecordingsContainerName = "recordings";
        public const string RemarksTableName = "remarks";
        public const string KeeperLogTableName = "keeperlog";
        public const string PaymentLogTableName = "paymentlog";
        public const string StatisticsTableName = "statistics";
        public const string TopicsTableName = "topics";

        public static string GetConnectionString()
        {
            // +http://blogs.msdn.com/b/windowsazure/archive/2013/07/17/windows-azure-web-sites-how-application-strings-and-connection-strings-work.aspx
            //return CloudConfigurationManager.GetSetting(StorageConnectionStringName);
            //return "UseDevelopmentStorage=true";
            //return System.Configuration.ConfigurationManager.AppSettings[StorageConnectionStringName];
            return System.Configuration.ConfigurationManager.ConnectionStrings[StorageConnectionStringName].ConnectionString;
        }

        public static CloudTableClient GetCloudTableClient()
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());
            return storageAccount.CreateCloudTableClient();
        }

        public static CloudTable GetCloudTable(string tableAddress)
        {
            // JSON NoMetadata client side type resolution +http://blogs.msdn.com/b/windowsazurestorage/archive/2013/12/05/windows-azure-tables-introducing-json.aspx
            ////tableClient.PayloadFormat = TablePayloadFormat.JsonNoMetadata; // Supported starting from version 3.0 of Windows Azure Storage Client

            return GetCloudTableClient().GetTableReference(tableAddress);
        }

        /// <summary>
        ///  We create Azure storage objects in ???WebRole.OnStart()??? Global.asax.cs/Application_Start
        /// </summary>
        public static void EnsureStorageObjectsExist()
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());

            // Blobs
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(AzureStorageUtils.RecordingsContainerName);
            if (!container.Exists())
            {
                container.CreateIfNotExists();
                // Configure container for public access
                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                container.SetPermissions(permissions);
            }

            // Tables
            var tableClient = storageAccount.CreateCloudTableClient();

            new List<string> {
                AzureStorageUtils.KeeperLogTableName,
                AzureStorageUtils.RemarksTableName,
                AzureStorageUtils.PaymentLogTableName,
                AzureStorageUtils.StatisticsTableName,
                AzureStorageUtils.TopicsTableName,
            }
            .ForEach(i =>
            {
                tableClient.GetTableReference(i).CreateIfNotExists();
            });
        }

        public static void InsertEntry(string tableAddress, ITableEntity entity)
        {
            var table = GetCloudTable(tableAddress);
            var insertOperation = TableOperation.Insert(entity);
            // var insertOperation = TableOperation.Insert(entity, echoContent: false) // No echo is the default behavior starting from version 3.0 of Windows Azure Storage Client Library. 
            table.Execute(insertOperation);
        }

        public static async Task InsertEntryAsync(string tableAddress, ITableEntity entity)
        {
            var table = GetCloudTable(tableAddress);
            var insertOperation = TableOperation.Insert(entity);
            // var insertOperation = TableOperation.Insert(entity, echoContent: false) // No echo is the default behavior starting from version 3.0 of Windows Azure Storage Client Library. 
            await table.ExecuteAsync(insertOperation);
        }

        public static CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(containerName);
        }

        public static bool IsLocalEmulator()
        {
            return GetConnectionString().Contains("UseDevelopmentStorage=true");
        }

        public static string IntToKey(int value)
        {
            return value.ToString("D10");
        }

        public static int KeyToInt(string value)
        {
            return Convert.ToInt32(value);
        }

        public static string DateTimeToKey(DateTime value)
        {
            return value.ToString("u"); // "yyyy-MM-dd HH:mm:ssZ"
        }

        public static string GetRecordingsBaseUrl()
        {
            var urlBase = IsLocalEmulator()
                ? "127.0.0.1:10000/devstoreaccount1"
                : "blob.englc.com"; // "englc.blob.core.windows.net";
            return string.Format("http://{0}/{1}/", urlBase, RecordingsContainerName);
        }







    }
}