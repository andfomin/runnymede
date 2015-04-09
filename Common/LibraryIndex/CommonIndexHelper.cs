using Dapper;
using Runnymede.Common.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Runnymede.Common.LibraryIndex
{

    public class CommonIndexHelper : LibraryIndexHelper
    {
        /* +http://msdn.microsoft.com/en-us/library/azure/dn798930.aspx
         * For large numbers of updates, batching of documents (up to 1000 documents per batch, or about 16 MB per batch) is recommended.  
         */
        private const int MaxCount = 1000;

        public CommonIndexHelper(string databaseConnectionString)
            : base(IndexKinds.CommonCollection, databaseConnectionString)
        {
        }

        protected override dynamic GetIndexDefinition()
        {
            var fields = new IndexField[] 
                { 
                    new IndexField { Name = "id",                Type = "Edm.String",             Key = true,  Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "format",            Type = "Edm.String",             Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "naturalKey",        Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "segment",           Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "title",             Type = "Edm.String",             Key = false, Searchable = true,  Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "categoryIds",       Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "categoryPathIds",   Type = "Collection(Edm.String)", Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = false },
                    new IndexField { Name = "categoryPathNames", Type = "Edm.String",             Key = false, Searchable = true,  Filterable = false, Sortable = false, Facetable = false, Retrievable = false },
                    new IndexField { Name = "tags",              Type = "Edm.String",             Key = false, Searchable = true,  Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "source",            Type = "Edm.String",             Key = false, Searchable = true,  Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasExplanation",    Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasExample",        Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasExercise",       Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasText",           Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasPicture",        Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasAudio",          Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "hasVideo",          Type = "Edm.Boolean",            Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },               
                    new IndexField { Name = "languageLevel",     Type = "Edm.Int32",              Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = false },               
                    new IndexField { Name = "rating",            Type = "Edm.Int32",              Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = false },               
            };

            var suggesters = new[]
            {
            new {
                Name = "sg",
                SearchMode = "analyzingInfixMatching",
                SourceFields = new []{"categoryPathNames", "tags", "source"}
                }
             };

            var scoringProfiles = new[] {
                new {
                    Name = "spMedRating",
                    FunctionAggregation = "sum", 
                    Text = new {
                        Weights = new {
                            categoryPathNames = 3,
                            Tags = 2,
                            Source = 2,
                            Title = 1,
                        },
                    },
                    Functions = new[] {
                        new {
                            Type = "magnitude",
                            Boost = 2,
                            FieldName = "languageLevel",
                            Interpolation = "constant",
                            Magnitude = new {
                                BoostingRangeStart = 64,
                                BoostingRangeEnd = 192,
                                ConstantBoostBeyondRange = false, 
                            },
                        },
                        new {
                            Type = "magnitude",
                            Boost = 2,
                            FieldName = "rating",
                            Interpolation = "linear", 
                            Magnitude = new {
                                BoostingRangeStart = 0,
                                BoostingRangeEnd = 255,
                                ConstantBoostBeyondRange = false, 
                            },
                        },
                    }, // Functions
                } // profile spMedRating
            };

            var definition = new
            {
                Name = IndexName,
                Fields = fields,
                Suggesters = suggesters,
                ScoringProfiles = scoringProfiles,
                DefaultScoringProfile = "spMedRating",
            };

            return definition;
        }

        private IEnumerable<Dictionary<string, object>> ResourcesToDocuments_Add(IEnumerable<CommonResource> resources)
        {
            // Action can be upload (default) | merge | mergeOrUpload | delete. +http://msdn.microsoft.com/en-us/library/azure/dn798930.aspx
            // Merge does retrieve old > combine old + delta = new > delete old > upload new. It reindexes the entire document, so the only benefit of using it is that you can send a portion of document.
            // But Liam Cavanagh recommends mergeOrUpload. +https://social.msdn.microsoft.com/Forums/azure/en-US/d411b70e-e85d-48ef-9317-a94a1771d48d/best-strategy-for-updatingreindex-the-whole-searchindex?forum=azuresearch
            return resources.Select(i =>
                new Dictionary<string, object>() {
                { "@search.action", "upload" },
                { "id", i.Id.ToString() },
                { "format", i.Format },
                { "naturalKey", i.NaturalKey },
                { "segment", i.Segment },
                { "title", i.Title },
                { "categoryIds", i.CategoryIds },
                { "categoryPathIds", i.CategoryPathIds },
                { "categoryPathNames", i.CategoryPathNames },
                { "tags", i.Tags },
                { "source", i.Source },
                { "hasExplanation", i.HasExplanation },
                { "hasExample", i.HasExample },
                { "hasExercise", i.HasExercise },
                { "hasText", i.HasText },
                { "hasPicture", i.HasPicture },
                { "hasAudio", i.HasAudio },
                { "hasVideo", i.HasVideo },
                { "languageLevel", i.LanguageLevel },
                { "rating", i.Rating },
                });
        }

        private IEnumerable<Dictionary<string, object>> ResourcesToDocuments_Delete(IEnumerable<CommonResource> resources)
        {
            return resources.Select(i =>
                new Dictionary<string, object>() {
                { "@search.action", "delete" },
                { "id", i.Id.ToString() },
                });
        }

        public override async Task IndexResources(bool reindexAllResources)
        {
            if (reindexAllResources)
            {
                FlagAllResourcesForIndexing();
            }

            var keepProcessing = true;
            while (keepProcessing)
            {
                var resourcesToAdd = await GetResourcesToAdd(MaxCount);
                var documentsToAdd = ResourcesToDocuments_Add(resourcesToAdd);

                var resourcesToDelete = await GetResourcesToDelete(MaxCount);
                var documentsToDelete = ResourcesToDocuments_Delete(resourcesToDelete);

                var resources = resourcesToAdd.Concat(resourcesToDelete);
                var documents = documentsToAdd.Concat(documentsToDelete);

                var count = documents.Count();

                if (count > 0)
                {
                    await SendDocuments(documents);
                    await ResetIndexedResources(resources);
                }
                keepProcessing = count >= MaxCount;
            }
        }

        private async Task<List<CommonResource>> GetResourcesToAdd(int maxCount)
        {
            var sql = @"
select Id, Format, NaturalKey, Segment, Title, CategoryIds, Tags, Source, 
    HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo, 
    LanguageLevel, Rating
from dbo.libGetResourcesToAddToIndex(@MaxCount);
";
            // .ToList() is intentional. We are going to process the items with LINQ extensively.
            var resources = (await SqlMapper.QueryAsync<CommonResource>(Connection, sql, new { MaxCount = maxCount, })).ToList();
            await PopulateCategoryPaths(resources);
            return resources;
        }

        private async Task<List<CommonResource>> GetResourcesToDelete(int maxCount)
        {
            var sql = @"
select Id
from dbo.libGetResourcesToDeleteFromIndex(@MaxCount);
";
            // .ToList() is intentional. We are going to do much LINQ processing with items
            return (await SqlMapper.QueryAsync<CommonResource>(Connection, sql, new { MaxCount = maxCount, })).ToList();
        }

        private async Task ResetIndexedResources(IEnumerable<CommonResource> resources)
        {
            var resourcesXml = new XElement("Resources",
                resources.Select(i => new XElement("R",
                    new XAttribute("Id", i.Id),
                    i.LanguageLevel.HasValue ? new XAttribute("Level", i.LanguageLevel) : null,
                    i.Rating.HasValue ? new XAttribute("Rating", i.Rating) : null
                    ))
                    )
                    .ToString(SaveOptions.DisableFormatting);

            await SqlMapper.ExecuteAsync(
                 Connection,
                 "dbo.libResetIndexedResources",
                 new { Resources = resourcesXml },
                 commandType: CommandType.StoredProcedure
                 );
        }

        private void FlagAllResourcesForIndexing()
        {
            var sql = @"
update libResources set ReindexSearch = 1 where IsCommon is not null;
";
            SqlMapper.Execute(Connection, sql);
        }

    }
}
