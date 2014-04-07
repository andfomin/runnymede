using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dapper;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System.Threading.Tasks;
using System.Data;
using Microsoft.AspNet.Identity;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    public class BalanceApiController : ApiController
    {

        // GET /api/balanceApi/?offset=0&limit=10
        public DataSourceDto<BalanceEntryDto> GetEntries(int offset, int limit)
        {
            var result = new DataSourceDto<BalanceEntryDto>();

            DapperHelper.QueryMultipleResiliently(
                "dbo.accGetEntries",
                new
                {
                    UserId = this.GetUserId(),
                    RowOffset = offset,
                    RowLimit = limit
                },
                CommandType.StoredProcedure,
                (Dapper.SqlMapper.GridReader reader) =>
                {
                    result.Items = reader.Read<BalanceEntryDto>().ToList();
                    result.TotalCount = reader.Read<int>().Single();
                });

            return result;
        }


    }
}
