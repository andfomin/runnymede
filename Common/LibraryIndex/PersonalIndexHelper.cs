using Dapper;
using Runnymede.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Runnymede.Common.LibraryIndex
{

    public class PersonalIndexHelper : LibraryIndexHelper
    {

        public PersonalIndexHelper(string databaseConnectionString)
            : base(IndexKinds.PersonalCollection, databaseConnectionString)
        {
        }

        protected override dynamic GetIndexDefinition()
        {
            var fields = new IndexField[] 
                { 
                    new IndexField { Name = "uId_rId",           Type = "Edm.String",             Key = true,  Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "userId",            Type = "Edm.Int32",              Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = false },               
                    new IndexField { Name = "id",                Type = "Edm.Int32",              Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },               
                    new IndexField { Name = "format",            Type = "Edm.String",             Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "naturalKey",        Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "segment",           Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "title",             Type = "Edm.String",             Key = false, Searchable = true,  Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "categoryIds",       Type = "Edm.String",             Key = false, Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
                    new IndexField { Name = "categoryPathIds",   Type = "Collection(Edm.String)", Key = false, Searchable = false, Filterable = true,  Sortable = false, Facetable = true,  Retrievable = true  },
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
                    new IndexField { Name = "comment",           Type = "Edm.String",             Key = false, Searchable = true,  Filterable = false, Sortable = false, Facetable = false, Retrievable = true  },
            };

            var suggesters = new[]
            {
            new {
                Name = "sg",
                SearchMode = "analyzingInfixMatching",
                SourceFields = new []{"categoryPathNames", "tags", "source"}
                }
             };

            //var scoringProfiles = new[] {
            //    new {
            //        Name = "spDefault",
            //        Text = new {
            //            Weights = new {
            //                CategoryPathNames = 3,
            //                Source = 2,
            //                Title = 1,
            //            },
            //        },
            //    } // profile spDefault
            //};

            var definition = new
            {
                Name = IndexName,
                Fields = fields,
                Suggesters = suggesters,
                //ScoringProfiles = scoringProfiles,
                //DefaultScoringProfile = "spDefault",
            };

            return definition;
        }

        private IEnumerable<Dictionary<string, object>> ResourcesToDocuments_Add(IEnumerable<PersonalResource> resources)
        {
            // Action can be upload (default) | merge | mergeOrUpload | delete. +http://msdn.microsoft.com/en-us/library/azure/dn798930.aspx
            // Merge does retrieve old > combine old + delta = new > delete old > upload new. It reindexes the entire document, so the only benefit of using it is that you can send a portion of document.
            // But Liam Cavanagh recommends mergeOrUpload. +https://social.msdn.microsoft.com/Forums/azure/en-US/d411b70e-e85d-48ef-9317-a94a1771d48d/best-strategy-for-updatingreindex-the-whole-searchindex?forum=azuresearch
            return resources.Select(i =>
                new Dictionary<string, object>() {
                { "@search.action", "upload" },
                { "uId_rId", i.UserId.ToString() + "_" + i.Id.ToString() },
                { "userId", i.UserId.ToString() },
                { "id", i.Id },        
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
                { "comment", i.Comment },
                });
        }

        private IEnumerable<Dictionary<string, object>> ResourcesToDocuments_Delete(IEnumerable<PersonalResource> resources)
        {
            return resources.Select(i =>
                new Dictionary<string, object>() {
                { "@search.action", "delete" },
                { "uId_rId", i.UserId.ToString() + "_" + i.Id.ToString() },
                });
        }

        public override async Task IndexResources(bool reindexAllResources = false)
        {
            if (reindexAllResources)
            {
                FlagAllResourcesForIndexing();
            }

            var resourcesToAdd = await GetResourcesToAdd();
            var documentsToAdd = ResourcesToDocuments_Add(resourcesToAdd);

            var resourcesToDelete = await GetResourcesToDelete();
            var documentsToDelete = ResourcesToDocuments_Delete(resourcesToDelete);

            var documents = documentsToAdd.Concat(documentsToDelete);

            if (documents.Count() > 0)
            {
                await SendDocuments(documents);
                var resources = resourcesToAdd.Concat(resourcesToDelete);
                await ResetIndexedResourcesAsync(resources);
            }
        }

        private async Task<List<PersonalResource>> GetResourcesToAdd(int userId = 0, int resourceId = 0)
        {
            var single = userId != 0 && resourceId != 0;

            // The table-valued function is inlined. So the query optimizer is able to apply the parameters and perform clustered index seek.
            var sql = @"
select UserId, Id, Format, NaturalKey, Segment, Title, CategoryIds, Tags, Source, 
    HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo,
    Comment
from dbo.libGetUserResourcesToAddToIndex()"
                + (
                single
                ? " where UserId = @UserId and Id = @ResourceId;"
                : ";"
                );

            var param = single
                ? new { UserId = userId, ResourceId = resourceId }
                : null;

            // We use .Result because an obscure internal exception occurs if "await" is used with .ToList().
            var res = SqlMapper.QueryAsync<PersonalResource>(Connection, sql, param).Result;
            // .ToList() is intentional. We are going to do much LINQ processing with items. 
            var resources = res.ToList();
            await PopulateCategoryPaths(resources);
            return resources;
        }

        private async Task<List<PersonalResource>> GetResourcesToDelete()
        {
            var sql = @"
select UserId, Id
from dbo.libGetUserResourcesToDeleteFromIndex();
";
            // .ToList() is intentional. We are going to do much LINQ processing with items
            return (await SqlMapper.QueryAsync<PersonalResource>(Connection, sql)).ToList();
        }

        private async Task ResetIndexedResourcesAsync(IEnumerable<PersonalResource> resources)
        {
            var userResourcesXml = new XElement("UserResources",
                resources.Select(i => new XElement("UR",
                    new XAttribute("U", i.UserId),
                    new XAttribute("R", i.Id)
                    )))
                    .ToString(SaveOptions.DisableFormatting);

            await SqlMapper.ExecuteAsync(
                 Connection,
                 "dbo.libResetIndexedUserResources",
                 new { @UserResources = userResourcesXml },
                 commandType: CommandType.StoredProcedure
                 );
        }

        private void FlagAllResourcesForIndexing()
        {
            var sql = @"
update libUserResources set ReindexSearch = 1;
";
            SqlMapper.Execute(Connection, sql);
        }

        /// <summary>
        /// Add or delete a particular personal resource to/from index
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="userId"></param>
        /// <param name="resourceId"></param>
        /// <param name="isPersonal">Flag determining whether to add or delete the resouurce.</param>S
        /// <returns></returns>
        public async Task IndexUserResource(CancellationToken ct, int userId, int resourceId, bool isPersonal)
        {
            List<PersonalResource> resources;
            IEnumerable<Dictionary<string, object>> documents;

            if (isPersonal)
            {
                resources = await GetResourcesToAdd(userId, resourceId);
                documents = ResourcesToDocuments_Add(resources);
            }
            else
            {
                resources = new List<PersonalResource>
                {
                    new PersonalResource {
                        UserId = userId,
                        Id = resourceId,
                    }
                };
                documents = ResourcesToDocuments_Delete(resources);
            }

            if (resources.Count() > 0 && !ct.IsCancellationRequested)
            {
                await SendDocuments(documents);
                await ResetIndexedResourcesAsync(resources);
            }
        }

    }
}
