var App;
(function (App) {
    (function (Utils) {
        var AngularGlobal = (function () {
            function AngularGlobal() {
            }
            AngularGlobal.$SCOPE = '$scope';
            AngularGlobal.$COOKIE_STORE = '$cookieStore';
            AngularGlobal.NG_COOKIES = 'ngCookies';
            AngularGlobal.$HTTP = '$http';
            return AngularGlobal;
        })();
        Utils.AngularGlobal = AngularGlobal;

        function logNgHttpError(data, status) {
            var m = data.exceptionMessage ? data.exceptionMessage : (data.message ? data.message : (data.error_description ? data.error_description : ''));
            toastr.error('Error. ' + m);
        }
        Utils.logNgHttpError = logNgHttpError;

        function ngHttpGet(http, url, successCallback) {
            http.get(url, { headers: App.Utils.getSecurityHeader() }).success(successCallback).error(App.Utils.logNgHttpError);
        }
        Utils.ngHttpGet = ngHttpGet;

        function ngHttpPost(http, url, data, successCallback, finallyCallback) {
            http.post(url, data, { headers: App.Utils.getSecurityHeader() }).success(successCallback).error(App.Utils.logNgHttpError).finally(finallyCallback);
        }
        Utils.ngHttpPost = ngHttpPost;

        function ngHttpPut(http, url, data, successCallback, finallyCallback) {
            http.put(url, data, { headers: App.Utils.getSecurityHeader() }).success(successCallback).error(App.Utils.logNgHttpError).finally(finallyCallback);
        }
        Utils.ngHttpPut = ngHttpPut;

        // Source: book AngularJS of O'Reilly. +http://proquestcombo.safaribooksonline.com.ezproxy.torontopubliclibrary.ca/book/programming/javascript/9781449355852/8dot-cheatsheet-and-recipes/chapter_8_pagination_html
        function PaginatorFactory() {
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
        Utils.PaginatorFactory = PaginatorFactory;
    })(App.Utils || (App.Utils = {}));
    var Utils = App.Utils;
})(App || (App = {}));

var appUtilsNg = angular.module("AppUtilsNg", []);
appUtilsNg.factory('Paginator', App.Utils.PaginatorFactory);
//# sourceMappingURL=utils-ng.js.map
