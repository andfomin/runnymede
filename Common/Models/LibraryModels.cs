using Newtonsoft.Json;
using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Common.Models
{

    public class Resource
    {
        // Come from database or Azure Search
        public int Id { get; set; }
        public string Format { get; set; }
        public string NaturalKey { get; set; } // naturalKey:regex(^[0-9_-a-zA-Z}]{11}$)
        public string Segment { get; set; }
        public string Title { get; set; }
        public string CategoryIds { get; set; } // nullable
        public string Tags { get; set; }
        public string SourceId { get; set; }
        public string Source { get; set; }
        public bool HasExplanation { get; set; }
        public bool HasExample { get; set; }
        public bool HasExercise { get; set; }
        public bool HasText { get; set; }
        public bool HasPicture { get; set; }
        public bool HasAudio { get; set; }
        public bool HasVideo { get; set; }

        public static string BuildSegment(int start, int finish)
        {
            return (start % 100000).ToString("d5") + (finish % 100000).ToString("d5");
        }
    }

    // Despite the name DTOs are not really objects at all. +http://blog.ploeh.dk/2011/05/31/AttheBoundaries,ApplicationsareNotObject-Oriented/
    public class ResourceDto : Resource
    {
        public string Url { get; set; } // It is passed when a new resource is added.
        // Personal info from dbo.libUserResources
        public bool IsPersonal { get; set; } // not nullable. So the mere presense of an IsPersonal value means the resource has been viewed.
        public string Comment { get; set; }
        public byte? LanguageLevelRating { get; set; } // nullable. Understood: 1 - almost nothing, 2 - almost everything, 3 - everything
        // Not persisted. 
        // We do not store the Viewed value explicitly in the database. Since isPersonal is not nullable in the DB, the mere presense of an isPersonal value means the resource has been viewed. It may be calculated on-the-fly in stored procedures.
        public bool Viewed { get; set; }
        // Comes from History (Table Storage)
        public string LocalTime { get; set; }
    }

    public class SearchableResource : Resource
    {
        // Calculated by the indexer
        public string CategoryPathNames { get; set; }
        public IEnumerable<string> CategoryPathIds { get; set; }
    }

    public class CommonResource : SearchableResource
    {
        // Come from database
        public byte? LanguageLevel { get; set; }
        public byte? Rating { get; set; }
    }

    public class PersonalResource : SearchableResource
    {
        // Come from database or Azure Search
        public int UserId { get; set; }
        public string Comment { get; set; }
    }

    public class AzureSearchResults
    {
        [JsonProperty("@odata.count")]
        public int ODataCount { get; set; }
        [JsonProperty("value")]
        public IEnumerable<ResourceDto> Value { get; set; }
    }

    // Data Type/Properties Matrix. +http://gauravmantri.com/2014/09/17/azure-search-service-some-documentedundocumented-business-rules/
    internal class IndexField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Key { get; set; }
        public bool Searchable { get; set; }
        public bool Filterable { get; set; }
        public bool Sortable { get; set; }
        public bool Facetable { get; set; }
        public bool Retrievable { get; set; }
        public bool Suggestions { get; set; }
    }

    public class LibraryHistoryEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey = KeyUtils.IntToKey(userId)
        // RowKey = KeyUtils.LocalTimeToInvertedKey(localTime), We keep the local time and order last records first for retrieval. 
        public int ResourceId { get; set; }
    }





}