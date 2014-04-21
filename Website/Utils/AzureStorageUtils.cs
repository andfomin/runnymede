using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Runnymede.Website.Utils
{
    public class AzureStorageUtils
    {
        public class ContainerNames
        {
            public const string Recordings = "recordings";
            public const string AvatarsLarge = "avatars-large";
            public const string AvatarsSmall = "avatars-small";
        }

        public class TableNames
        {
            public const string Remarks = "remarks";
            public const string KeeperLog = "keeperlog";
            public const string PaymentLog = "paymentlog";
            public const string Statistics = "statistics";
            public const string Topics = "topics";
        }

        public const string StorageConnectionStringName = "StorageConnection";
        private const string AzureStorageDomainName = "blob.englc.com"; // "englc.blob.core.windows.net";

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

            new List<string>
            {
                ContainerNames.Recordings,
                ContainerNames.AvatarsLarge,
                ContainerNames.AvatarsSmall,
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
                AzureStorageUtils.TableNames.KeeperLog,
                AzureStorageUtils.TableNames.Remarks,
                AzureStorageUtils.TableNames.PaymentLog,
                AzureStorageUtils.TableNames.Statistics,
                AzureStorageUtils.TableNames.Topics,
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
            return value.ToString("D10"); // Prepend with zeroes.
        }

        public static int KeyToInt(string value)
        {
            return Convert.ToInt32(value);
        }

        public static string DateTimeToKey(DateTime value)
        {
            return value.ToString("u"); // "yyyy-MM-dd HH:mm:ssZ"
        }

        public static string GetContainerBaseUrl(string containerName, bool secure = false)
        {
            var isLocal = IsLocalEmulator();
            var baseUrl = isLocal
                            ? "127.0.0.1:10000/devstoreaccount1"
                            : AzureStorageDomainName;

            return string.Format("http{0}://{1}/{2}/", (secure && !isLocal) ? "s" : "",  baseUrl, containerName);
        }

        private static CloudBlockBlob InternalPrepareBlobUpload(Stream stream, string containerName, string blobName, string contentType)
        {
            var blobContainer = AzureStorageUtils.GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            if (!string.IsNullOrEmpty(contentType))
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
    }
}