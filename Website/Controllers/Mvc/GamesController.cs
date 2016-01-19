using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Website.Controllers.Mvc
{
    public class GamesController : Runnymede.Website.Utils.CustomController
    {
        // GET: games/pick-a-pic
        public ActionResult PickAPic()
        {
            return View();
        }

        // GET: games/copycat
        public ActionResult Copycat()
        {
            return View();
        }

        // GET: games/copycat-add
        public ActionResult CopycatAdd()
        {
            return View();
        }

        // GET: games/lucky-you
        public async Task<ActionResult> LuckyYou()
        {
            this.EnsureExtIdCookie();

            const string sql = @"
select [Date], Digits from dbo.lckGetDigits();
";
            var digits = await DapperHelper.QueryResilientlyAsync<LuckyDigits>(sql);
            return View(digits);
        }

    }
}