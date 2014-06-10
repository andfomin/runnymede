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

            bundles.Add(new StyleBundle("~/bundles/app-css").Include(
                 "~/bower_installer/toastr/toastr.min.css",
                 "~/bower_installer/angular-loading-bar/loading-bar.min.css",
                 "~/Content/site.css"
                 ));

            bundles.UseCdn = false;

            //var jqueryCdnPath = "//ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js";
            bundles.Add(new ScriptBundle("~/bundles/jquery"/*, jqueryCdnPath*/).Include(
                //"~/Scripts/jquery-{version}.js"
                "~/bower_installer/jquery/jquery.min.js"
                ));

            var swfobjectCdnPath = "//ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js";
            bundles.Add(new ScriptBundle("~/bundles/swfobject", swfobjectCdnPath).Include(
                        "~/Content/Audior/swfobject.js"));

            bundles.Add(new ScriptBundle("~/bundles/soundmanager2").Include(
                    "~/Scripts/sm2/soundmanager2-nodebug-jsmin.js"
                ));

            // //ajax.aspnetcdn.com/ajax/bootstrap/3.0.3/bootstrap.min.js
            // netdna.bootstrapcdn.com/bootstrap/3.0.3/js/bootstrap.min.js
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/bower_installer/bootstrap/bootstrap.min.js"
                ));

            // +https://ajax.googleapis.com/ajax/libs/angularjs/1.2.15/MANIFEST
            // //ajax.googleapis.com/ajax/libs/angularjs/1.0.2/angular.js
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                //"~/bower_components/angular/angular.js"
                "~/bower_installer/angular/angular.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/angular-bootstrap").Include(
                "~/bower_installer/angular-bootstrap/ui-bootstrap-tpls.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/angular-libs").Include(
                "~/bower_installer/angular-route/angular-route.min.js",
                // 3rd party
                "~/bower_installer/angular-loading-bar/loading-bar.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/ext-libs").Include(
                "~/bower_installer/moment/moment.min.js",
                "~/bower_installer/toastr/toastr.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/app-utils").Include(
                "~/App/Shared/utils.js",
                "~/App/Shared/model.js",
                "~/App/Shared/utils-ng.js"
                ));

        }
    }
}
