using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace Runnymede.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit +http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // If you'd like to test the optimization locally, you can use this line to force it.
            //BundleTable.EnableOptimizations = true;
            bundles.UseCdn = false;

            //cdnjs.cloudflare.com/ajax/libs/fullcalendar/2.2.3/fullcalendar.min.js
            //cdnjs.cloudflare.com/ajax/libs/fullcalendar/2.2.3/fullcalendar.min.css

            bundles.Add(new StyleBundle("~/bundles/app-css").Include(
                 "~/bower_installer/toastr/toastr.min.css",
                 "~/bower_installer/angular-loading-bar/loading-bar.min.css",
                 "~/content/styles/site.css"
                 ));

            // +http://cdnjs.cloudflare.com/ajax/libs/animate.css/3.1.0/animate.css
            bundles.Add(new StyleBundle("~/bundles/animate-css").Include(
                 "~/bower_installer/animate.css/animate.min.css"
                 ));

            //var jqueryCdnPath = "//ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js";
            bundles.Add(new ScriptBundle("~/bundles/jquery"/*, jqueryCdnPath*/).Include(
                //"~/scripts/jquery-{version}.js"
                "~/bower_installer/jquery/jquery.min.js"
                ));

            var swfobjectCdnPath = "//ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js";
            // The source js file is already minified.
            bundles.Add(new ScriptBundle("~/bundles/swfobject", swfobjectCdnPath).Include(
                        //"~/content/audior/swfobject.js"));
                        "~/bower_installer/swfobject/swfobject.js"));

            bundles.Add(new ScriptBundle("~/bundles/soundmanager2").Include(
                    //"~/scripts/sm2/soundmanager2-nodebug-jsmin.js"
                    "~/scripts/sm2/soundmanager2.js"
                ));

            // //ajax.aspnetcdn.com/ajax/bootstrap/3.0.3/bootstrap.min.js
            // //maxcdn.bootstrapcdn.com/bootstrap/3.2.0/js/bootstrap.min.js
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/bower_installer/bootstrap/bootstrap.min.js"
                ));

            // +https://ajax.googleapis.com/ajax/libs/angularjs/1.2.15/MANIFEST
            // //ajax.googleapis.com/ajax/libs/angularjs/1.0.2/angular.js
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/bower_components/angular/angular.js"
                //"~/bower_installer/angular/angular.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/angular-libs").Include(
                "~/bower_installer/angular-animate/angular-animate.min.js",
                "~/bower_installer/angular-sanitize/angular-sanitize.min.js",
                // Angular UI
                "~/bower_installer/angular-bootstrap/ui-bootstrap-tpls.min.js",
                "~/bower_installer/angular-ui-router/angular-ui-router.min.js",
                // 3rd party
                "~/bower_installer/angular-loading-bar/loading-bar.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/ext-libs").Include(
                "~/bower_installer/moment/moment.min.js",
                "~/bower_installer/toastr/toastr.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/app-utils").Include(
                "~/app/shared/utils.js",
                "~/app/shared/utils-ng.js"
                //"~/app/shared/utils-signalR.js"
                ));

        }
    }
}
