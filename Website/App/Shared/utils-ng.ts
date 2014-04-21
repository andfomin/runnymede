module App.Utils {

    export interface IScopeWithViewModel extends ng.IScope {
        vm: any;
    }

    export class ngNames {
        // from module ng
        public static $scope = '$scope';
        public static $http = '$http';
        public static $log = '$log';
        public static $filter = '$filter';
        public static $rootScope = '$rootScope';
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
                ngHttpGetWithParamsNoCache($http,
                    App.Utils.sessionsApiUrl('MillisSinceEpoch'),
                    null,
                    (data) => {
                        $rootScope.isClockWrong = (Math.abs(new Date().valueOf() - data) > 60000); // Calculate the difference between the client and the provided time
                    }
                    );
            }]);
    }

    export function ngHttpGet(http: ng.IHttpService, url: string, successCallback: ng.IHttpPromiseCallback<any>) {
        http.get(url)
            .success(successCallback)
            .error(App.Utils.logError);
    }

    // $http does not send data in body in a GET request, only as params.
    export function ngHttpGetWithParams(http: ng.IHttpService, url: string, params: any, successCallback: ng.IHttpPromiseCallback<any>) {
        http.get(url, { params: params })
            .success(successCallback)
            .error(App.Utils.logError);
    }

    export function ngHttpGetWithParamsNoCache(http: ng.IHttpService, url: string, params: any, successCallback: ng.IHttpPromiseCallback<any>) {
        var ps = params || {};
        ps._ = safeDateNow();
        ngHttpGetWithParams(http, url, ps, successCallback);
    }

    export function ngHttpPost(http: ng.IHttpService, url: string, data: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        http.post(url, data)
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
        http.put(url, data)
            .success(successCallback)
            .error(App.Utils.logError)
            .finally(finallyCallback);
    }

    // Source: book AngularJS of O'Reilly. +http://proquestcombo.safaribooksonline.com.ezproxy.torontopubliclibrary.ca/book/programming/javascript/9781449355852/8dot-cheatsheet-and-recipes/chapter_8_pagination_html
    export function PaginatorFactory() {
        // Despite being a factory, the user of the service gets a new
        // Paginator every time he calls the service. This is because
        // we return a function that provides an object when executed
        // The fetchFunction function expects the following signature: fetchFunction(offset, limit, callback); When the data is available, the fetch function needs to call the callback function with it.
        /* 
        <a href="" ng-click="searchPaginator.previous()" ng-show="searchPaginator.hasPrevious()">&lt;&lt; Prev</a>
        <a href="" ng-click="searchPaginator.next()" ng-show="searchPaginator.hasNext()">Next &gt;&gt;</a> 
        */

        return function (fetchFunction, pageSize) {
            var paginator = {
                hasNextVar: false,
                next: function () {
                    if (this.hasNextVar) {
                        this.currentOffset += pageSize;
                        this._load();
                    }
                },
                _load: function () {
                    var self = this;
                    fetchFunction(this.currentOffset, pageSize + 1, function (items) {
                        self.currentPageItems = items.slice(0, pageSize);
                        self.hasNextVar = items.length === pageSize + 1;
                    });
                },
                hasNext: function () {
                    return this.hasNextVar;
                },
                previous: function () {
                    if (this.hasPrevious()) {
                        this.currentOffset -= pageSize;
                        this._load();
                    }
                },
                hasPrevious: function () {
                    return this.currentOffset !== 0;
                },
                currentPageItems: [],
                currentOffset: 0
            };

            // Load the first page
            paginator._load();
            return paginator;
        };
    } // end of PaginatorFactory

    // We inject the $filter service to be able to call the standard filter internally.
    export function AppDateFilterFactory($filter) {
        return (date) => {
            var f = $filter('date'); // The standard filter.
            return f(date, 'fullDate') + ' ' + f(date, 'shortTime');
        }
    }

} // end of module App.Utils

var appUtilsNg = angular.module('AppUtilsNg', []);
appUtilsNg.factory('Paginator', App.Utils.PaginatorFactory);
appUtilsNg.filter('appDate', [App.Utils.ngNames.$filter, App.Utils.AppDateFilterFactory]);

