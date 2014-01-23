module App.Utils {

    export interface IScopeWithViewModel extends ng.IScope {
        vm: any;
    }

    export class AngularGlobal {
        public static $SCOPE = '$scope';
        public static $COOKIE_STORE = '$cookieStore';
        public static NG_COOKIES = 'ngCookies';
        public static $HTTP = '$http';
    }

    export function logNgHttpError(data, status) {
        var m = data.exceptionMessage ? data.exceptionMessage : (data.message ? data.message : (data.error_description ? data.error_description : ''));
        toastr.error('Error. ' + m);// + ' Status code ' + status);
    }

    export function ngHttpGet(http: ng.IHttpService, url: string, successCallback: ng.IHttpPromiseCallback<any>) {
        http.get(
            url,
            { headers: App.Utils.getSecurityHeader() }
            )
            .success(successCallback)
            .error(App.Utils.logNgHttpError);
    }

    export function ngHttpPost(http: ng.IHttpService, url: string, data: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        http.post(
            url,
            data,
            { headers: App.Utils.getSecurityHeader() }
            )
            .success(successCallback)
            .error(App.Utils.logNgHttpError)
            .finally(finallyCallback);
    }

    export function ngHttpPut(http: ng.IHttpService, url: string, data: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        http.put(
            url,
            data,
            { headers: App.Utils.getSecurityHeader() }
            )
            .success(successCallback)
            .error(App.Utils.logNgHttpError)
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
    }
} // end of module

var appUtilsNg = angular.module("AppUtilsNg", []);
appUtilsNg.factory('Paginator', App.Utils.PaginatorFactory);

