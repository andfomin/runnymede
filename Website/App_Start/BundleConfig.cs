using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace Runnymede.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // If you'd like to test the optimization locally, you can use this line to force it.
            //BundleTable.EnableOptimizations = true;

            bundles.Add(new StyleBundle("~/content/css-app").Include(
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
                "~/Scripts/jquery-{version}.js"
                //"~/Scripts/jquery-2.0.3.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui"/*, jqueryCdnPath*/).Include(
                "~/Scripts/xcopy/jquery-ui-1.10.3.custom.js"
                //"~/Scripts/xcopy/jquery-ui-1.10.3.custom.min.js"
                ));

            // Knockout 3.0.0 loses binding in the Account/Create form on button click.
            //var knockoutCdnPath = "//ajax.aspnetcdn.com/ajax/knockout/knockout-2.3.0.js";
            bundles.Add(new ScriptBundle("~/bundles/knockout"/*, knockoutCdnPath*/).Include(
                "~/Scripts/knockout-2.3.0.debug.js",
                "~/Scripts/knockout-2.3.0.js"
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

            //bundles.Add(new ScriptBundle("~/bundles/breeze").Include(
            //    "~/Scripts/q.js",
            //    // Web Optimization will only bundle one or the other of the Breeze libraries depending upon whether you build the application for debug or release.
            //    "~/Scripts/breeze.debug.js",
            //    "~/Scripts/breeze.min.js"
            //    ));

            // //ajax.aspnetcdn.com/ajax/bootstrap/3.0.3/bootstrap.min.js
            // netdna.bootstrapcdn.com/bootstrap/3.0.3/js/bootstrap.min.js
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js"
                ));

            ////bundles.Add(new ScriptBundle("~/bundles/knockout-lib").Include(
            ////    "~/Scripts/knockout.validation.js",
            ////    "~/Scripts/knockout.activity.js",
            ////    "~/Scripts/knockout.command.js",
            ////    "~/Scripts/knockout.dirtyFlag.js"
            ////    //"~/Scripts/ko.datasource.js",
            ////    ));

            //+//ajax.googleapis.com/ajax/libs/angularjs/1.0.2/angular.js
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-route.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/ext-libs").Include(
                "~/Scripts/moment.js",
                "~/Scripts/toastr.js"
                ));

            /*----------- Old pages -----------*/

            bundles.Add(new ScriptBundle("~/bundles/jsextlibs").Include(
                // Knockout plugins
                "~/Scripts/knockout.activity.js",
                "~/Scripts/knockout.command.js",
                "~/Scripts/knockout.dirtyFlag.js",
                //"~/scripts/lib/knockout.validation.js",
                "~/Scripts/ko.datasource.js"
                // Other 3rd party libraries
                //"~/Scripts/moment.js",
                //"~/Scripts/toastr.js"
                //"~/scripts/lib/jquery.html5-placeholder-shim.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/App/Utils/utils.js",
                "~/App/Utils/model.js"
                ));

            /*----------- End old pages -----------*/

            bundles.Add(new ScriptBundle("~/bundles/app-utils").Include(
                "~/App/Utils/utils.js",
                "~/App/Utils/model.js",
                "~/App/Utils/utils-ng.js"
                ));

        }
    }
}
