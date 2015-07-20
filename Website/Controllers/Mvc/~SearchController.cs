using Bing;
using Dapper;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class SearchController : Controller
    {

        private class SearchResult
        {
            public string Domain { get; set; }
            public string Url { get; set; }
            public int UserBoost { get; set; }
            public int GlobalBoost { get; set; }
            public int OriginalPosition { get; set; }
        }

        private async Task<ActionResult> InternalSearch(string q)
        {
            IEnumerable<SearchResult> viewModel = new List<SearchResult>();

            if (String.IsNullOrWhiteSpace(q))
            {
                return View(viewModel);
            }

            string query = HttpUtility.UrlDecode(q);

            int queryId = 0;
            IEnumerable<SearchResult> boosts = new List<SearchResult>();

            // Lookup the query cache in the database.
            var querySql = @"
select coalesce(PrimaryQueryId, Id) as QueryId from dbo.resQueries where Query = @Query;
";
            var boostsSql = @"
select Domain, Boost as UserBoost, 0 as GlobalBoost from dbo.resSourceBoosts where UserId = @UserId
union all
select Domain, 0 as UserBoost, Boost as GlobalBoost from dbo.resSourceBoosts where UserId = 0;
";
            var globalBoostsSql = @"
select Domain, 0 as UserBoost, Boost as GlobalBoost from dbo.resSourceBoosts where UserId = 0;
";
            var isAuthenticated = User.Identity.IsAuthenticated;

            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                queryId = (await connection.QueryAsync<int>(querySql, new { Query = query })).SingleOrDefault();

                if (queryId != 0)
                {
                    // We have found a cached copy of the query.
                }

                boosts = await connection.QueryAsync<SearchResult>(
                    isAuthenticated ? boostsSql : globalBoostsSql,
                    isAuthenticated ? new { UserId = this.GetUserId() } : null);
            }

            // Hit Bing
            if (queryId == 0)
            {
                var client = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Data.ashx/Bing/Search/v1/"));
                client.Credentials = new NetworkCredential("accountKey", "5YaDOM5e+yVu5m0YTagnldui36VI5px9STCbUrpZJCI");

                // API Reference +http://msdn.microsoft.com/en-us/library/dd250882.aspx
                // Bing Query Language +http://msdn.microsoft.com/en-us/library/ff795667.aspx
                var bingQuery = client.Composite(
                    "web", //"web+video+relatedsearch+spell", // Sources
                    query,//	"ireglr veb (site:englishpage.com OR site:esl.about.com)", // Query
                    "DisableLocationDetection", // Options
                    null, //"DisableHostCollapsing", // WebSearchOptions
                    "en-US", // Market
                    "Strict", // Adult
                    null, // Latitude
                    null, // Longitude
                    null, // WebFileType
                    null, // ImageFilters
                    null, // VideoFilters
                    "Relevance", // VideoSortBy
                    null, // NewsLocationOverride
                    null, // NewsCategory
                    null // NewsSortBy
                    );

                //var bingResults = bingQuery.Execute();
                // TPL and Traditional .NET Framework Asynchronous Programming +http://msdn.microsoft.com/en-us/library/dd997423(v=vs.110).aspx
                var bingResults = await Task<IEnumerable<ExpandableSearchResult>>.Factory.FromAsync(bingQuery.BeginExecute, bingQuery.EndExecute, null);

                var webResults = (bingResults as QueryOperationResponse<ExpandableSearchResult>).First().Web;

                // Simplify DisplayUrl. Keep only the main domain part.
                var results = webResults
                    .Select((i, idx) =>
                    {
                        var url = i.DisplayUrl;
                        var ix = url.IndexOf("://"); // DisplayUrl can contain an "https://".
                        url = ix > 0 ? url.Substring(ix + 3) : url;
                        url = url.StartsWith("www.") ? url.Substring(4) : url;
                        ix = url.IndexOf('/');
                        url = ix > 0 ? url.Substring(0, ix) : url;
                        return new SearchResult { Domain = url, Url = i.Url, OriginalPosition = idx };
                    });

                // Sort the results according to the boost value. Some results may have a boost counterpart, some not.
                // And vice versa, some boosts may have no result to apply to. We filter out such groups, where there is no Url, i.e. no result.
                viewModel = results
                    .Concat(boosts)
                    .GroupBy(i => i.Domain)
                    //.Where(i => i.Any(j => !String.IsNullOrEmpty(j.Url)))
                    .Select(i =>
                        {
                            var res = i.FirstOrDefault(j => !String.IsNullOrEmpty(j.Url));
                            return res != null
                                ? new SearchResult
                                    {
                                        Domain = i.Key,
                                        //Url = i.Select(j => j.Url).FirstOrDefault(j => !String.IsNullOrEmpty(j)),
                                        Url = res.Url,
                                        //OriginalPosition = i.Sum(j => j.OriginalPosition),
                                        OriginalPosition = res.OriginalPosition,
                                        UserBoost = i.Sum(j => j.UserBoost),
                                        GlobalBoost = i.Sum(j => j.GlobalBoost),
                                    }
                                : null;
                        })
                    .Where(i => i != null)
                    .OrderByDescending(i => i.UserBoost)
                    .ThenByDescending(i => i.GlobalBoost)
                    .ThenBy(i => i.OriginalPosition);
            }

            return View(viewModel);
        }


    }
}