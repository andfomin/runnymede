using Dapper;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Runnymede.Common.LibraryIndex
{
    public abstract class LibraryIndexHelper : IDisposable
    {

        private class CategoryPath
        {
            public string Id { get; set; }
            public string NamePath { get; set; }
            public string IdPath { get; set; }
            public IEnumerable<string> PathNames { get; set; }
            public IEnumerable<string> PathIds { get; set; }
        }

        /* +http://msdn.microsoft.com/en-us/library/azure/dn798941.aspx
* The index name must be lower case, start with a letter or number, have no slashes or dots, and be less than 128 characters. 
* After starting the index name with a letter or number, the rest of the name can include any letter, number and dashes, as long as the dashes are not consecutive.         
*/
        public enum IndexKinds
        {
            CommonCollection,
            PersonalCollection
        };

        private const string CommonCollectionIndexNameKey = "CommonCollectionIndexName";
        private const string PersonalCollectionIndexNameKey = "PersonalCollectionIndexName";

        private Lazy<SqlConnection> lazyConnection;
        protected SqlConnection Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public string IndexName { get; private set; }

        public LibraryIndexHelper(IndexKinds indexKind, string databaseConnectionString)
        {
            lazyConnection = new Lazy<SqlConnection>(() =>
            {
                var connection = new SqlConnection(databaseConnectionString);
                connection.Open();
                return connection;
            });

            IndexName = GetIndexName(indexKind);
        }

        public static string GetIndexName(IndexKinds indexKind)
        {
            string indexNameKey;
            switch (indexKind)
            {
                case IndexKinds.CommonCollection:
                    indexNameKey = CommonCollectionIndexNameKey;
                    break;
                case IndexKinds.PersonalCollection:
                    indexNameKey = PersonalCollectionIndexNameKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(indexKind.ToString());
            }
            return ConfigurationManager.AppSettings[indexNameKey];
        }

        protected virtual dynamic GetIndexDefinition()
        {
            throw new NotImplementedException();
        }

        public abstract Task IndexResources(bool reindexAllResources);

        private async Task<List<CategoryPath>> GetCategoryPaths(IEnumerable<Resource> resources)
        {
            // A resource can belong in many categories. The Resouce-Category relationship is many-to-many. It is stored in dbo.libCategoryResources.
            // Load the paths for all the categories involved in the processed resources.
            var categoryIds = resources
                .Where(i => !String.IsNullOrEmpty(i.CategoryIds))
                .Select(i => i.CategoryIds.Split(' '))
                .SelectMany(i => i)
                .Distinct()
                ;

            var categoriesXml = new XElement("Categories",
                categoryIds.Select(i => new XElement("C", new XAttribute("Id", i)))
                )
                .ToString(SaveOptions.DisableFormatting);

            const string sql = @"
select Id, NamePath, IdPath from dbo.libGetCategoryPaths (@Categories);
";
            // .ToList() is intentional. We are going to do much LINQ processing with items
            var categories = (await SqlMapper.QueryAsync<CategoryPath>(Connection, sql, new { Categories = categoriesXml })).ToList();

            categories.ForEach(i =>
            {
                i.PathNames = i.NamePath.Split(new[] { "||" }, StringSplitOptions.None);
                i.PathIds = i.IdPath.Split(' ');
            });

            return categories;
        }

        protected async Task PopulateCategoryPaths(IEnumerable<SearchableResource> resources)
        {
            if (resources.Count() > 0)
            {
                var categoryPaths = await GetCategoryPaths(resources);
                // Transfer paths from categoryPaths to the resources.
                foreach (var r in resources)
                {
                    if (!String.IsNullOrEmpty(r.CategoryIds))
                    {
                        var paths = r.CategoryIds
                            .Split(' ')
                            .Join(categoryPaths, s => s, o => o.Id, (s, o) => o);

                        var pathNames = paths
                            .SelectMany(i => i.PathNames)
                            .Distinct();

                        r.CategoryPathNames = String.Join(" ", pathNames);

                        var pathIds = paths
                            .SelectMany(i => i.PathIds)
                            .Distinct();

                        r.CategoryPathIds = pathIds;
                    }
                    else
                    {
                        // type 'Collection(Edm.String)[Nullable=False]' does not allow null values.
                        r.CategoryPathIds = new string[] { };
                    }
                }
            }
        }

        protected async Task SendDocuments(IEnumerable<Dictionary<string, object>> documents)
        {
            var batch = new
            {
                value = documents
            };
            var azureSearchHelper = new AzureSearchHelper(IndexName, 0);
            var response = await azureSearchHelper.SendDocumentsAsync(batch);
            var json = await response.Content.ReadAsStringAsync();

            var results = JObject.Parse(json);
            var value = results["value"];
            var statuses = value.Children();
            var errors = statuses.Where(i => !(bool)i["status"]);
            if (errors.Any())
            {
                var groups = errors.GroupBy(i => (string)i["errorMessage"]);
                var first = groups.First();
                throw new Exception(String.Format("Error count: total {0}, distinct {1}. First error: key {2}, message {3}",
                    errors.Count(), groups.Count(), (string)first.First()["key"], first.Key));
            }
        }

        public void RecreateIndex()
        {
            var indexDefinition = GetIndexDefinition();
            var azureSearchHelper = new AzureSearchHelper(IndexName, 0);
            azureSearchHelper.RecreateIndex(indexDefinition);
        }

        public void Dispose()
        {
            if (this.lazyConnection.IsValueCreated)
            {
                this.lazyConnection.Value.Close();
            }
        }
    }
}
