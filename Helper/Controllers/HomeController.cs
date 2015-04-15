using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Helper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connectionString = RoleEnvironment.GetConfigurationSettingValue(RecordingsController.StorageConnectionSetting);
            ViewBag.Length = connectionString.Length;
            return View();
        }
    }
}
