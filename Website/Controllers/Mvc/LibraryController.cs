using Microsoft.WindowsAzure.Storage.Table;
using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{

    public class LibraryController : Runnymede.Website.Utils.CustomController
    {
        // GET: /resources
        public ActionResult Index()
        {
            return View();
        }

        // GET: /history
        public async Task<ActionResult> History()
        {
            // Send all the days there are records for. We will enable/disable days in the calendar on the page accordingly. RowKeys in the table are "inverted" local time.
            var days = new List<string>();

            if (this.IsAuthenticated())
            {
                var userId = this.GetUserId();
                var partitionKey = KeyUtils.IntToKey(userId);
                var filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                var query = new TableQuery<TableEntity>().Where(filterPartition);
                var table = AzureStorageUtils.GetCloudTable(AzureStorageUtils.TableNames.LibraryHistory);

                TableQuerySegment<TableEntity> currentSegment = null;
                while (currentSegment == null || currentSegment.ContinuationToken != null)
                {
                    currentSegment = await table.ExecuteQuerySegmentedAsync<TableEntity>(
                        query,
                        currentSegment != null ? currentSegment.ContinuationToken : null
                        );

                    // Format 2014-01-21 as "140121"
                    var localDays = currentSegment.Results
                        .GroupBy(i => i.RowKey.Substring(0, 6))
                        .Select(i => KeyUtils.InvertedKeyToLocalTime(i.Key, 3, "", "d2").Substring(2))
                        ;

                    days.AddRange(localDays);
                }
            }

            var daysParam = days.Distinct();
            ViewBag.DaysParamJson = JsonUtils.SerializeAsJson(daysParam);
            return View();
        }

        // GET: /friend
        public ActionResult Friend()
        {
            return View();
        }

    }

}