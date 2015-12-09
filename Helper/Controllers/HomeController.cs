using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Runnymede.Helper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var connectionString = RoleEnvironment.GetConfigurationSettingValue(RecordingsController.StorageConnectionSetting);
            var connectionString = ConfigurationManager.ConnectionStrings[RecordingsController.StorageConnectionSetting].ConnectionString;
            ViewBag.Length = connectionString.Length;
            return View();
        }
    }
}
