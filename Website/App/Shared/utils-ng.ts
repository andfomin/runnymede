module App.Utils {

    export interface IScopeWithViewModel extends ng.IScope {
        vm: any;
    }

    export class ngNames {
        // from module ng
        public static $scope = '$scope';
        public static $http = '$http';
        public static $log = '$log';
        public static $q = '$q';
        public static $filter = '$filter';
        public static $rootScope = '$rootScope';
        public static $timeout = '$timeout';
        public static $interval = '$interval'; // Intervals created by this service must be explicitly destroyed. See the example in the docs.
        // ngRoute 
        public static $route = '$route';
        public static $routeParams = '$routeParams';
        public static $location = '$location';
        // ui-bootstrap
        public static $modal = '$modal';
        public static $modalInstance = '$modalInstance';
    }

    export interface IAppRootScopeService extends ng.IRootScopeService {
        // Dynamically change the page title. ?Refactor as a service? See +http://stackoverflow.com/a/17751833/2808337
        pageTitle: string;
        isClockWrong: boolean;
    }

    export class CustomModal {

        executing: boolean = false;

        internalOk: () => ng.IPromise<any> = null; // Override in descendand classes.

        /* +http://www.typescriptlang.org/Content/TypeScript%20Language%20Specification.pdf  Section 8.2.3
        *  Base class static property members can be overridden by derived class static property members of any kind as long as the types are compatible, as described above. */
        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modalInstance, 'modalParams'];

        constructor(
            public $scope: App.Utils.IScopeWithViewModel,
            public $http: ng.IHttpService,
            public $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            public modalParams: any
            ) {
            $scope.vm = this;
        } // ctor

        ok = () => {
            if (this.internalOk) {
                this.executing = true;
                this.internalOk()
                    .then<void>(
                    () => { this.$modalInstance.close(); },
                    App.Utils.logError
                    )
                    .finally(() => { this.executing = false; });
            }
            else {
                this.$modalInstance.close();
            }
        }

        cancel = () => {
            this.$modalInstance.dismiss('cancel');
        };

        /**
         * The successCallback parameter should be passed with the lambda syntax, otherwise the _this will be lost.
         */
        public static openModal(
            $modal: ng.ui.bootstrap.IModalService,
            templateUrl: string,
            controller: any,
            modalParams: any,
            successCallback: () => void
            ) {
            var modalInstance = $modal.open({
                templateUrl: templateUrl,
                controller: controller,
                resolve: {
                    modalParams: () => {
                        return modalParams;
                    }
                }
            });
            modalInstance.result.then(
                () => {
                    if (successCallback) {
                        successCallback();
                    }
                },
                null
                );
            return modalInstance;
        }
    }; // end of class CustomModal

    export function useRouteTitle(app: ng.IModule) {
        app.run([ngNames.$rootScope, function ($rootScope: IAppRootScopeService) {
            $rootScope.$on('$routeChangeSuccess', (event, current, previous) => {
                $rootScope.pageTitle = current.title;
                //window.document.title = current.title + ' \u002d English Cosmos'; // &ndash;
            });
        }]);
    }

    export function detectWrongClock(app: ng.IModule) {
        app.run([
            ngNames.$http,
            ngNames.$rootScope,
            function ($http: ng.IHttpService, $rootScope: IAppRootScopeService) {
                ngHttpGetNoCache($http,
                    App.Utils.sessionsApiUrl('MillisSinceEpoch'),
                    null,
                    (data) => {
                        $rootScope.isClockWrong = (Math.abs(new Date().valueOf() - data) > 60000); // Calculate the difference between the client and the provided time
                    }
                    );
            }]);
    }

    // $http does not send data in body in a GET request, only as params.
    export function ngHttpGetNoCache(http: ng.IHttpService, url: string, params: any, successCallback: ng.IHttpPromiseCallback<any>) {
        var ps = params || {};
        ps._ = safeDateNow();
        return http.get(url, { params: ps })
            .success(successCallback)
            .error(App.Utils.logError);
    }

    export function ngHttpPost(http: ng.IHttpService, url: string, data: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        return http.post(url, data)
            .success(successCallback)
            .error(App.Utils.logError)
            .finally(finallyCallback);
    }

    /* How to post a form-encoded, not JSON-encoded, data */
    //    $http({
    //        method: 'POST',
    //        url: Utils.loginUrl(),
    //        data: {
    //            userName: userName,
    //        },
    //        /* The OWIN authintication middleware does not accept JSON. It wants a form. */
    //        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
    //        transformRequest: function (obj) {
    //            var str = [];
    //            for (var p in obj)
    //                str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
    //            return str.join("&");
    //        }
    //    })

    export function ngHttpPut(http: ng.IHttpService, url: string, data: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        return http.put(url, data)
            .success(successCallback)
            .error(App.Utils.logError)
            .finally(finallyCallback);
    }

    // Source: book AngularJS of O'Reilly. +http://proquestcombo.safaribooksonline.com.ezproxy.torontopubliclibrary.ca/book/programming/javascript/9781449355852/8dot-cheatsheet-and-recipes/chapter_8_pagination_html
    ////export function PaginatorFactory() {
    ////    // Despite being a factory, the user of the service gets a new
    ////    // Paginator every time he calls the service. This is because
    ////    // we return a function that provides an object when executed
    ////    // The fetchFunction function expects the following signature: fetchFunction(offset, limit, callback); When the data is available, the fetch function needs to call the callback function with it.
    ////    /* 
    ////    <a href="" ng-click="searchPaginator.previous()" ng-show="searchPaginator.hasPrevious()">&lt;&lt; Prev</a>
    ////    <a href="" ng-click="searchPaginator.next()" ng-show="searchPaginator.hasNext()">Next &gt;&gt;</a> 
    ////    */

    ////    return function (fetchFunction, pageSize) {
    ////        var paginator = {
    ////            hasNextVar: false,
    ////            next: function () {
    ////                if (this.hasNextVar) {
    ////                    this.currentOffset += pageSize;
    ////                    this._load();
    ////                }
    ////            },
    ////            _load: function () {
    ////                var self = this;
    ////                fetchFunction(this.currentOffset, pageSize + 1, function (items) {
    ////                    self.currentPageItems = items.slice(0, pageSize);
    ////                    self.hasNextVar = items.length === pageSize + 1;
    ////                });
    ////            },
    ////            hasNext: function () {
    ////                return this.hasNextVar;
    ////            },
    ////            previous: function () {
    ////                if (this.hasPrevious()) {
    ////                    this.currentOffset -= pageSize;
    ////                    this._load();
    ////                }
    ////            },
    ////            hasPrevious: function () {
    ////                return this.currentOffset !== 0;
    ////            },
    ////            currentPageItems: [],
    ////            currentOffset: 0
    ////        };

    ////        // Load the first page
    ////        paginator._load();
    ////        return paginator;
    ////    };
    ////} // end of PaginatorFactory

    // We inject the $filter service to be able to call the standard filter internally.
    export function AppDateFilterFactory($filter) {
        return (date: Date) => {
            var f = $filter('date'); // The standard filter.
            return angular.isDefined(date) ? (f(date, 'fullDate') + ' ' + f(date, 'shortTime')) : null;
        }
    }

    export function AppDateTimeFilterFactory() {
        return (date: Date) => {
            return angular.isDefined(date) ? moment(date).format(App.Utils.dateTimeFormat) : null;
        }
    }

    // Format exercise length from milliseconds to min:sec
    export function AppMsecToMinSecFilterFactory() {
        return (valMsec: number) => {
            return App.Utils.formatMsec(valMsec);
        }
    }

} // end of module App.Utils

var appUtilsNg = angular.module('AppUtilsNg', []);
////appUtilsNg.factory('Paginator', App.Utils.PaginatorFactory);
appUtilsNg.filter('appDate', [App.Utils.ngNames.$filter, App.Utils.AppDateFilterFactory]);
appUtilsNg.filter('appDateTime', [App.Utils.AppDateTimeFilterFactory]);
appUtilsNg.filter('appMsecToMinSec', [App.Utils.AppMsecToMinSecFilterFactory]);

