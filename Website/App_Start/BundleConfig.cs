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

            bundles.Add(new StyleBundle("~/content/css-jqueryui").Include(
                "~/Content/jqueryui/jquery.ui.core.css",
                "~/Content/jqueryui/jquery.ui.slider.css",
                "~/Content/jqueryui/jquery.ui.theme.css"
                ));


            bundles.UseCdn = false;

            //var jqueryCdnPath = "//ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js";
            bundles.Add(new ScriptBundle("~/bundles/jquery"/*, jqueryCdnPath*/).Include(
                //"~/Scripts/jquery-{version}.js"
                "~/bower_installer/jquery/jquery.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui"/*, jqueryCdnPath*/).Include(
                //"~/Scripts/xcopy/jquery-ui-1.10.3.custom.js"
                "~/Scripts/xcopy/jquery-ui-1.10.3.custom.min.js"
                ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at +http://modernizr.com to pick only the tests you need.
            // CDN +http://yandex.st/modernizr/2.6.2/modernizr.min.js
            // +http://ajax.aspnetcdn.com/ajax/modernizr/modernizr-2.0.6-development-only.js
            ////bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            ////    "~/Scripts/modernizr-*"
            ////    ));

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

            /*----------- Old pages -----------*/

            // Knockout 3.0.0 loses binding in the Account/Create form on button click. Thus we use the previous version 2.3.0.
            //var knockoutCdnPath = "//ajax.aspnetcdn.com/ajax/knockout/knockout-2.3.0.js";
            bundles.Add(new ScriptBundle("~/bundles/knockout"/*, knockoutCdnPath*/).Include(
                //"~/Scripts/knockout-2.3.0.debug.js",
                //"~/Scripts/knockout-2.3.0.js"
                "~/bower_installer/knockout.js/knockout.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/ko-libs").Include(
                // Knockout plugins
                "~/bower_installer/kolite/knockout.command.js",
                "~/bower_installer/kolite/knockout.dirtyFlag.js",
                //"~/bower_components/kolite/knockout.activity.js", // The new version drops the jquery plugin support
                "~/Scripts/xcopy/knockout.activity.js",
                "~/bower_installer/ko.datasource/ko.datasource.js"
                //"~/scripts/lib/knockout.validation.js",
                // Not KO-related
                //"~/scripts/lib/jquery.html5-placeholder-shim.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/App/Shared/utils.js",
                "~/App/Shared/model.js"
                ));

            /*----------- End old pages -----------*/

            bundles.Add(new ScriptBundle("~/bundles/app-utils").Include(
                "~/App/Shared/utils.js",
                "~/App/Shared/model.js",
                "~/App/Shared/utils-ng.js"
                ));

        }
    }
}
