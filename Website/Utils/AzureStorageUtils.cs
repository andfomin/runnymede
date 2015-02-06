using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Website.Utils
{
    public class AzureStorageUtils
    {
        #region Storage object names

        // Naming and Referencing Containers, Blobs, and Metadata +http://msdn.microsoft.com/en-us/library/azure/dd135715.aspx
        public class ContainerNames
        {
            public const string Recordings = "recordings";
            public const string WritingPhotos = "writing-photos";
            public const string AvatarsLarge = "user-avatars-large";
            public const string AvatarsSmall = "user-avatars-small";
            public const string Presentations = "user-presentations";
        }

        // Table name rules are described by the regular expression "^[A-Za-z][A-Za-z0-9]{2,62}$". Table names are case-insensitive. No dashes. +http://msdn.microsoft.com/library/azure/dd179338.aspx
        public class TableNames
        {
            public const string ReviewPieces = "reviewpieces";
            public const string KeeperLog = "keeperlog";
            public const string PaymentLog = "paymentlog";
            public const string UserMessages = "usermessages";
            public const string GamePicapick = "gamepickapic";
            public const string GameCopycat = "gamecopycat";
            public const string LibraryHistory = "libraryhistory"; // Partitioned by UserId
            public const string LibraryLog = "librarylog"; // Partitioned by time
        }

        // The queue name must be a valid DNS name. All letters must be lowercase. Naming Queues and Metadata +http://msdn.microsoft.com/en-us/library/dd179349.aspx
        public class QueueNames
        {
            public const string IndexPersonal = "index-personal";
        }

        #endregion

        private const string StorageConnectionStringName = "StorageConnection";
        public const string WebJobsConnectionStringName = "AzureWebJobsStorage";

        // Do not alias the Blob domain name. Custom domain mapping does not support HTTPS, which means HTTP blob links will trigger secutrity notifications on HTTPS pages.
        public const string AzureBlobHostname = "englmdata.blob.core.windows.net";

        public const string DefaultDirectoryDelimiter = "/"; // in ctor CloudBlobClient(), this.DefaultDelimiter = NavigationHelper.Slash;

        public static string GetConnectionString(string connectionStringName = null)
        {
            // +http://blogs.msdn.com/b/windowsazure/archive/2013/07/17/windows-azure-web-sites-how-application-strings-and-connection-strings-work.aspx
            //return "UseDevelopmentStorage=true";
            return System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName ?? StorageConnectionStringName].ConnectionString;
        }

        /// <summary>
        ///  We create Azure storage objects in Global.asax.cs/Application_Start
        /// </summary>
        public static void EnsureStorageObjectsExist()
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());

            // Blobs
            var blobClient = storageAccount.CreateCloudBlobClient();
            new List<string>
            {
                ContainerNames.Recordings,
                ContainerNames.WritingPhotos,
                ContainerNames.AvatarsLarge,
                ContainerNames.AvatarsSmall,
                ContainerNames.Presentations,
            }
            .ForEach(i =>
            {
                var container = blobClient.GetContainerReference(i);
                if (!container.Exists())
                {
                    container.CreateIfNotExists();
                    // Configure container for public access
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                    container.SetPermissions(permissions);
                }
            });

            // Tables
            var tableClient = storageAccount.CreateCloudTableClient();
            new List<string> {
                TableNames.KeeperLog,
                TableNames.ReviewPieces,
                TableNames.PaymentLog,
                TableNames.UserMessages,
                TableNames.LibraryHistory,
                TableNames.LibraryLog,
                TableNames.GamePicapick,
                TableNames.GameCopycat,
            }
            .ForEach(i =>
            {
                tableClient.GetTableReference(i).CreateIfNotExists();
            });

            // Queues
            var queueClient = storageAccount.CreateCloudQueueClient();
            new List<string>
            {
                //QueueNames.IndexPersonal,
            }
            .ForEach(i =>
            {
                queueClient.GetQueueReference(i).CreateIfNotExists();
            });
        }

        public static bool IsLocalEmulator()
        {
            return GetConnectionString().Contains("UseDevelopmentStorage=true");
        }

        #region Table

        public static CloudTableClient GetCloudTableClient()
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());
            var tableClient = storageAccount.CreateCloudTableClient();
            // JSON NoMetadata client side type resolution +http://blogs.msdn.com/b/windowsazurestorage/archive/2013/12/05/windows-azure-tables-introducing-json.aspx
            // When using JSON No metadata via the Table Service Layer the client library will “infer” the property types by inspecting the type information on the POCO entity type. The client will inspect an entity type once and cache the resulting information.
            tableClient.DefaultRequestOptions.PayloadFormat = TablePayloadFormat.JsonNoMetadata;
            return tableClient;
        }

        public static CloudTable GetCloudTable(string tableName)
        {
            return GetCloudTableClient().GetTableReference(tableName);
        }

        public static void InsertEntity(string tableAddress, ITableEntity entity)
        {
            var table = GetCloudTable(tableAddress);
            var insertOperation = TableOperation.Insert(entity);
            table.Execute(insertOperation);
        }

        public static async Task InsertEntityAsync(string tableAddress, ITableEntity entity)
        {
            var table = GetCloudTable(tableAddress);
            var operation = TableOperation.Insert(entity);
            await table.ExecuteAsync(operation);
        }

        public static async Task InsertOrReplaceEntityAsync(string tableAddress, ITableEntity entity)
        {
            var table = GetCloudTable(tableAddress);
            var operation = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(operation);
        }

        public static async Task<List<T>> ExecuteQueryAsync<T>(string tableName, TableQuery<T> query) where T : ITableEntity, new()
        {
            // +http://stackoverflow.com/questions/26257822/azure-table-query-async-continuation-token-always-returned
            var takeCount = query.TakeCount.GetValueOrDefault(Int32.MaxValue);
            var table = AzureStorageUtils.GetCloudTable(tableName);
            var list = new List<T>();
            TableQuerySegment<T> currentSegment = null;
            while ((currentSegment == null || currentSegment.ContinuationToken != null) && (list.Count() < takeCount))
            {
                currentSegment = await table.ExecuteQuerySegmentedAsync<T>(
                    query,
                    currentSegment != null ? currentSegment.ContinuationToken : null
                    );
                list.AddRange(currentSegment.Results);
            }
            return list;
        }

        #endregion

        #region Blob

        public static CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString());
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(containerName);
        }

        public static CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var blobContainer = AzureStorageUtils.GetCloudBlobContainer(containerName);
            return blobContainer.GetBlockBlobReference(blobName);
        }

        //public static string GetContainerBaseUrl(string containerName, bool secure = false)
        //{
        //    var isLocal = IsLocalEmulator();
        //    var protocol = (secure && !isLocal) ? "https" : "http";
        //    var baseUrl = isLocal
        //                    ? "127.0.0.1:10000/devstoreaccount1"
        //                    : AzureBlobDomainName;

        //    return string.Format("{0}://{1}/{2}/", protocol, baseUrl, containerName);
        //}

        public static string GetBlobUrl(string containerName, string blobName, bool secure = false)
        {
            var isLocal = IsLocalEmulator();
            var protocol = secure ? "https" : "http";
            var hostname = isLocal 
                ? ConfigurationManager.AppSettings["DevHostname"] 
                : AzureBlobHostname;
            var format = isLocal
                            ? "{0}://{1}/api/exercises/artifact/{2}?blobName={3}"
                            : "{0}://{1}/{2}/{3}";
            return String.Format(format, protocol, hostname, containerName, blobName);
        }

        private static CloudBlockBlob InternalPrepareBlobUpload(Stream stream, string containerName, string blobName, string contentType)
        {
            var blob = GetBlob(containerName, blobName);
            if (!String.IsNullOrEmpty(contentType))
            {
                blob.Properties.ContentType = contentType;
            }
            if (stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            return blob;
        }

        public static void UploadBlob(Stream stream, string containerName, string blobName, string contentType)
        {
            var blob = InternalPrepareBlobUpload(stream, containerName, blobName, contentType);
            blob.UploadFromStream(stream);
        }

        public static async Task UploadBlobAsync(Stream stream, string containerName, string blobName, string contentType)
        {
            var blob = InternalPrepareBlobUpload(stream, containerName, blobName, contentType);
            await blob.UploadFromStreamAsync(stream);
        }

        // Depricated. There is direct CloudBlockBlob.DownloadTextAsync()
        //public static async Task<string> GetBlobAsText(string containerName, string blobName)
        //{
        //    var blob = GetBlob(containerName, blobName);
        //    string text = null;
        //    if (await blob.ExistsAsync())
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await blob.DownloadToStreamAsync(memoryStream);
        //            text = Encoding.UTF8.GetString(memoryStream.ToArray());
        //        }
        //    }
        //    return text;
        //}

        #endregion

        #region Queue

        public static async Task AddMessageAsync(string queueName, string content, string connectionStringName = null)
        {
            var storageAccount = CloudStorageAccount.Parse(GetConnectionString(connectionStringName));
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            var message = new CloudQueueMessage(content);
            await queue.AddMessageAsync(message);
        }

        #endregion

        //return new[] { fromTime.ToString("u") };
        // PartitionKey eq '0000000004' and RowKey ge '2013-12-14 20:48:26Z0000000021' and RowKey le '2013-12-14 20:48:26Z0000000021'
        //PartitionKey eq '0000000004' and RowKey ge '2013-12-14 20:48:26Z' and RowKey le '2013-12-14 20:48:26ZA'
        // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 20:48:26Z') and (RowKey le '2013-12-14 20:48:26ZA'))
        // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 15:48:26Z') and (RowKey le '2013-12-14 15:48:26ZA'))
        // (PartitionKey eq '0000000004') and ((RowKey ge '2013-12-14 15:48:26Z') and (RowKey le '2013-12-14 15:48:26ZA'))
        //var filterFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, AzureStorageUtils.DateTimeToKey(fromTime));
        //var filterTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, AzureStorageUtils.DateTimeToKey(toTime) + "A"); // 'A' is greater than any digit in the alphabetical order.


    }
}