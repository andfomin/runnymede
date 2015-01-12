using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Runnymede.Website.Utils;
using System.IO;
using System.Data;
using Runnymede.Website.Models;
using Dapper;
using Newtonsoft.Json;
using System.Xml.Linq;
using Runnymede.Common.Models;
using Runnymede.Common.LibraryIndex;
using System.Web.Hosting;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Common.Utils;

/* +https://developers.google.com/youtube/v3/docs/videos/list
 * id, snippet, contentDetails, liveStreamingDetails, recordingDetails, statistics, status, topicDetails
 * +http://www.freebase.com/m/02h40lc - English Language 
 * +http://www.freebase.com/m/0jt6_33 - English as a second or foreign language
 * 01b5n5 - Vocabulary. 01h8n0 - Conversation
 */

namespace Runnymede.Website.Controllers.Api
{
    [RoutePrefix("api/library")]
    public class LibraryApiController : ApiController
    {

        // GET /api/library/search_common?offset=0&limit=10&categoryId=0123_&q=qwerty+asdfg+zxcvbn
        [Route("common")]
        public async Task<IHttpActionResult> GetCommon(int offset = 0, int limit = 10, string categoryId = null, string q = null)
        {
            return await InternalSearch(LibraryIndexHelper.IndexKinds.CommonCollection, offset, limit, categoryId, q);
        }

        // GET /api/library/search_personal?offset=0&limit=10&categoryId=0123_&q=qwerty+asdfg+zxcvbn
        [Route("personal")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> GetPersonal(int offset = 0, int limit = 10, string categoryId = null, string q = null)
        {
            return await InternalSearch(LibraryIndexHelper.IndexKinds.PersonalCollection, offset, limit, categoryId, q);
        }

        private async Task<IHttpActionResult> InternalSearch(LibraryIndexHelper.IndexKinds indexKind, int offset, int limit, string categoryId = null, string q = null)
        {
            if (limit > AzureSearchHelper.SearchResults_MaxLimit)
            {
                throw new ArgumentException(String.Format("Search results limit is {0}.", AzureSearchHelper.SearchResults_MaxLimit));
            }

            var isPersonal = indexKind == LibraryIndexHelper.IndexKinds.PersonalCollection;
            var userId = this.GetUserId();
            // Find resources in Azure Search.
            var search = new AzureSearchHelper(indexKind, isPersonal ? userId : 0);
            var resultsJson = await search.Search(offset, limit, categoryId, q);

            if (isPersonal || (userId == 0))
            {
                return new RawStringResult(this, resultsJson, RawStringResult.TextMediaType.Json);
            }
            else
            {
                // Common collection for an authorized user. Augment the results with personal data from database.
                var results = JsonConvert.DeserializeObject<AzureSearchResults>(resultsJson);

                if (results.Value.Any())
                {
                    var resourcesXml = new XElement("Resources",
                        results.Value.Select(i => new XElement("R", new XAttribute("Id", i.Id)))
                        )
                        .ToString(SaveOptions.DisableFormatting);

                    const string sql = @"
select Id, IsPersonal, LanguageLevelRating, Comment 
from dbo.libGetUserResources (@UserId, @Resources);
";
                    var views = await DapperHelper.QueryResilientlyAsync<ResourceDto>(sql, new { UserId = userId, Resources = resourcesXml });

                    // Copy personalization values from the items we got from the database to the resources we got from search.
                    var resourceDict = results.Value.ToDictionary(i => i.Id);
                    foreach (var v in views)
                    {
                        // We expect the Ids returned from the database correspond to the Ids provided as arguments in XML. Otherwise a horrible KeyNotFoundException will ruine everything.
                        var r = resourceDict[v.Id];
                        // We do not store the Viewed value explicitly in the database. IsPersonal is not nullable. So the mere presense of an IsPersonal in a record means the resource has been viewed.
                        r.IsPersonal = v.IsPersonal;
                        r.LanguageLevelRating = v.LanguageLevelRating;
                        r.Comment = v.Comment;
                        r.Viewed = true;
                    }
                }

                // Since we send original raw Azure Search results in the "Personal" case as is, we use here Ok<AzureSearchResults> as well to substitute names from attributes for member names, i.e. "@odata.count" and "value".
                return Ok<AzureSearchResults>(results);
            }
        }

        // GET /api/library/personal_categories
        [Route("personal_categories")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> GetPersonalCategories()
        {
            var indexName = LibraryIndexHelper.GetIndexName(LibraryIndexHelper.IndexKinds.PersonalCollection);
            var searchHelper = new AzureSearchHelper(indexName, this.GetUserId());
            //var json = await searchHelper.GetPersonalCategories();
            var json = await searchHelper.Search(0, 1000, null, "*", false, new[] { "categoryPathIds" });
            return new RawStringResult(this, json, RawStringResult.TextMediaType.Json);
        }

        // GET /api/library/category/01234_/exponents
        [Route("category/{categoryId:length(4)}/exponents")]
        public async Task<IHttpActionResult> GetExponents(string categoryId)
        {
            const string sql = @"
select CategoryId, ReferenceLevel, [Text]
from dbo.libExponents
where CategoryId = @CategoryId
order by ReferenceLevel;
";
            var exponents = await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { CategoryId = categoryId });
            return Ok(exponents);
        }

        // GET /api/library/history?offset=0&limit=10
        [Route("day_history")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> GetDayHistory(int offset, int limit, string day)
        {
            object result = null;

            if (this.IsAuthenticated())
            {
                var userId = this.GetUserId();

                // RowKeys are inverse to time value.
                var dayInvertedKey = KeyUtils.LocalTimeToInvertedKey(day);
                if (String.IsNullOrEmpty(dayInvertedKey))
                {
                    throw new ArgumentOutOfRangeException(day);
                }
                // Include the entire day.
                var dayRowKey = dayInvertedKey.Substring(0, 6);
                var rowKeyFrom = dayRowKey + "000000";
                var rowKeyTo = dayRowKey + "235959";

                var filterRowFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
                var filterRowTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyTo);
                string combinedRowFilter = TableQuery.CombineFilters(filterRowFrom, TableOperators.And, filterRowTo);

                result = await GetHistoryResources(userId, combinedRowFilter, offset, limit);
            }

            return Ok(result);
        }

        // GET /api/library/last_history
        [Route("last_history")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> GetLastHistory(int offset, int limit)
        {
            var result = await GetHistoryResources(this.GetUserId(), null, offset, limit);
            return Ok(result);
        }

        // GET /api/library/friend_history/12345
        [Route("friend_history/{id:int}")]
        public async Task<IHttpActionResult> GetFriendHistory(int id)
        {
            object result = null;

            if (this.IsAuthenticated())
            {
                var userId = this.GetUserId();

                // Check if the users are friends.
                const string sql = @"
select convert(bit, count(*)) as IsActive
from dbo.friGetFriends(@UserId)
where Id = @FriendUserId and UserIsActive = 1 and FriendIsActive = 1;
";
                var isActive = (await DapperHelper.QueryResilientlyAsync<bool>(sql, new { UserId = userId, FriendUserId = id })).Single();

                if (!isActive)
                {
                    return BadRequest("Friendship is not mutually active.");
                }

                // Limit table scan by row key. Look up in the last day history to cover any timezone and mistakes with local clock.
                var localTime = DateTime.UtcNow.AddDays(-1).EncodeLocalTime();
                // RowKeys are inverse to time value.
                var rowKey = KeyUtils.LocalTimeToInvertedKey(localTime);

                // One hour is hardcoded within the message in app.library.Friend.load();
                var timestampValue = DateTimeOffset.UtcNow.AddHours(-1);

                var filterRow = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKey);
                var filterTimestamp = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, timestampValue);
                string combinedFilter = TableQuery.CombineFilters(filterRow, TableOperators.And, filterTimestamp);

                result = await GetHistoryResources(id, combinedFilter, 0, 10);
            }
            return Ok(result);
        }

        // PUT /api/library/   // Add a resource to the user's personal collection.
        [Route("")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PostResource(ResourceDto resource)
        {
            ResourceDto result;

            // In the case of a new resource Id is not passed. In the case of a common resource Id has a value.
            if (resource.Id == 0)
            {
                var newResource = await LibraryUtils.TryValidateResource(resource.Url);

                // If validation is unsuccessfull, TryValidateResource returns null
                if (newResource != null)
                {
                    resource.Format = newResource.Format;
                    resource.NaturalKey = newResource.NaturalKey;
                    resource.Title = newResource.Title ?? resource.Title;
                    resource.HasVideo = newResource.HasVideo || resource.HasVideo;
                }
                else
                {
                    resource.NaturalKey = null;
                }
            }

            if (resource.NaturalKey != null)
            {
                var userId = this.GetUserId();
                // Idempotent
                result = (await DapperHelper.QueryResilientlyAsync<ResourceDto>("dbo.libCreatePersonalResource",
                      new
                      {
                          UserId = userId,
                          Id = resource.Id,
                          Format = resource.Format,
                          NaturalKey = resource.NaturalKey,
                          Segment = resource.Segment,
                          Title = resource.Title,
                          CategoryIds = GeneralUtils.SanitizeSpaceSeparatedWords(resource.CategoryIds),
                          Tags = GeneralUtils.SanitizeSpaceSeparatedWords((resource.Tags ?? "").Replace("-", String.Empty).ToLower()),
                          SourceId = resource.SourceId,
                          HasExplanation = resource.HasExplanation,
                          HasExample = resource.HasExample,
                          HasExercise = resource.HasExercise,
                          HasText = resource.HasText,
                          HasPicture = resource.HasPicture,
                          HasAudio = resource.HasAudio,
                          HasVideo = resource.HasVideo,
                          Comment = resource.Comment,
                      },
                      CommandType.StoredProcedure))
                      .SingleOrDefault();

                // Update the search index.
                var indexHelper = new PersonalIndexHelper(DapperHelper.GetConnectionString());
                if (resource.Id == 0)
                {
                    // A new resource is added on the Personal tab. It is waiting to update the category list. Update the index ASAP.
                    await indexHelper.IndexUserResource(CancellationToken.None, userId, result.Id, true);
                }
                else
                {
                    // If a common resource is added, update the search index in background. Return response early. 
                    //// If this task fails, the WebJob will process everything.
                    //var queueTask = AzureStorageUtils.QueueMessage(
                    //    AzureStorageUtils.WebJobsConnectionStringName,
                    //    AzureStorageUtils.QueueNames.IndexPersonal,
                    //    userId.ToString() + " " + resourceId.ToString()
                    //    );
                    //await Task.WhenAll(dbTask, queueTask);
                    HostingEnvironment.QueueBackgroundWorkItem(ct => indexHelper.IndexUserResource(ct, userId, result.Id, true));
                }
            }
            else
            {
                throw new ArgumentException("Wrong web address.");
            }

            return Ok(result);
        }

        // POST /api/library/resource_view
        [Route("resource_view/{id:int}")]
        public async Task<IHttpActionResult> PostResourceView([FromUri] int id, [FromBody] JObject value)
        {
            // A suggestion may have resourceId == null(0?) when the resource is made on the client in code and points to Google

            // TODO. Idea: Get the unauthenicated user's session cookie and log views for the session in a separate datastore.
            var userId = this.GetUserId(); // GetUserId() returns 0 for an unauthenticated user. That's fine. We log every view.   

            var logEnity = new DynamicTableEntity(KeyUtils.GetCurrentTimeKey(), KeyUtils.IntToKey(userId));
            logEnity["Json"] = new EntityProperty(value.ToString());
            var logTask = AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.LibraryLog, logEnity);

            if (userId != 0 && id != 0)
            {
                var viewTask = DapperHelper.ExecuteResilientlyAsync("dbo.libPostResourceView",
                     new
                     {
                         UserId = userId,
                         ResourceId = id,
                     },
                     CommandType.StoredProcedure);

                // We use KeyUtils.LocalTimeToInvertedKey() to keep the local time and to order last records first for retrieval.
                var localTime = (string)value["localTime"];
                var rowKey = KeyUtils.LocalTimeToInvertedKey(localTime);

                if (String.IsNullOrEmpty(rowKey))
                {
                    throw new ArgumentOutOfRangeException(localTime);
                }

                var historyEntity = new LibraryHistoryEntity
                {
                    PartitionKey = KeyUtils.IntToKey(userId),
                    RowKey = rowKey,
                    ResourceId = id,
                };
                var historyTask = AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.LibraryHistory, historyEntity);

                // Do all the tasks in parallel.
                await Task.WhenAll(viewTask, historyTask, logTask);
            }
            else
            {
                await logTask;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST /api/library/language_level_rating
        [Route("language_level_rating")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PostLanguageLevelRating([FromBody] JObject value)
        {
            await DapperHelper.ExecuteResilientlyAsync("dbo.libPostLanguageLevelRating",
                  new
                  {
                      UserId = this.GetUserId(),
                      ResourceId = (int)value["resourceId"],
                      LanguageLevelRating = (byte)value["languageLevelRating"],
                  },
                  CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.Created);
        }

        // POST /api/library/problem_report
        [Route("problem_report")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> PostProblemReport([FromBody] JObject value)
        {
            int? userId = this.GetUserId();
            if (userId.GetValueOrDefault() == 0)
            {
                userId = null;
            }

            JToken reportedVersion;
            int? resourceId = null;
            if (value.TryGetValue("reportedVersion", out reportedVersion))
            {
                resourceId = reportedVersion.Value<int?>("id");
            }

            (await DapperHelper.QueryResilientlyAsync<int>("dbo.libCreateProblemReport",
                new
                {
                    UserId = userId, // nullable
                    ResourceId = resourceId, // nullable
                    Report = value.ToString(Formatting.None, null),
                },
                CommandType.StoredProcedure))
                .Single();

            return StatusCode(HttpStatusCode.Created);
        }

        // DELETE /api/library/Personal/12345
        [Route("personal/{id:int}")]
        [AppPoliteAuthorize]
        public async Task<IHttpActionResult> DeletePersonalResource(int id)
        {
            var userId = this.GetUserId();
            await DapperHelper.ExecuteResilientlyAsync("dbo.libDeletePersonalResource",
                  new
                  {
                      UserId = userId,
                      ResourceId = id,
                  },
                  CommandType.StoredProcedure);

            var indexHelper = new PersonalIndexHelper(DapperHelper.GetConnectionString());
            await indexHelper.IndexUserResource(CancellationToken.None, userId, id, false);

            // The client immidiately re-requests personal categories at the end of the delete operation. Ensure Azure Search keeps pace. BTW, there is additional delay in JS on the client side.
            await Task.Delay(1000);

            return StatusCode(HttpStatusCode.NoContent);
        }

        private async Task<DapperHelper.PageItems<ResourceDto>> GetHistoryResources(int userId, string secondaryFilter, int offset = 0, int limit = 0)
        {
            var result = new DapperHelper.PageItems<ResourceDto>();

            var partitionKey = KeyUtils.IntToKey(userId);
            var filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var combinedFilter = !String.IsNullOrEmpty(secondaryFilter) ? TableQuery.CombineFilters(filterPartition, TableOperators.And, secondaryFilter) : filterPartition;
            var query = new TableQuery<LibraryHistoryEntity>().Where(combinedFilter);
            var entities = await AzureStorageUtils.ExecuteQueryAsync(AzureStorageUtils.TableNames.LibraryHistory, query);

            result.TotalCount = entities.Count();

            if (entities.Any())
            {
                var historyItems = (limit != 0)
                        ? entities.Skip(offset).Take(limit)
                        : entities;

                result.Items = await HydrateHistoryItems(historyItems, userId);
            }

            return result;
        }

        private async Task<IEnumerable<ResourceDto>> HydrateHistoryItems(IEnumerable<LibraryHistoryEntity> historyItems, int userId)
        {
            var resourcesXml = new XElement("Resources",
                historyItems
                .Select(i => i.ResourceId)
                .Distinct()
                .Select(i => new XElement("R", new XAttribute("Id", i)))
                )
                .ToString(SaveOptions.DisableFormatting);

            var sql = @"
select Id, Format, NaturalKey, Segment, Title, CategoryIds, Tags, Source, 
    HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo,
    IsPersonal, Comment, Viewed
from dbo.libGetResources(@UserId, @Resources);
";
            var resources = await DapperHelper.QueryResilientlyAsync<ResourceDto>(sql, new { UserId = userId, Resources = resourcesXml });

            // Correlate the resources we got from the database to the items we got from the history table.
            var dict = resources.ToDictionary(i => i.Id);

            return historyItems
                .Select(i =>
                {
                    ResourceDto resource;
                    if (dict.TryGetValue(i.ResourceId, out resource))
                    {
                        resource.LocalTime = KeyUtils.InvertedKeyToLocalTime(i.RowKey);
                    }
                    else
                    {
                        resource = new ResourceDto();
                    }
                    return resource;
                })
                .Where(i => i.Id != 0)
                ;
        }

    }
}
