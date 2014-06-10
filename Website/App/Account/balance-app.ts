module App.Account_Balance {

    export class Ctrl {

        entries: any[] = [];
        limit: number = 20; // items per page
        currentPage: number = 1;
        totalCount: number = 0;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.getEntries();
        } // end of ctor

        getEntries() {
            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.balanceApiUrl(),
                {
                    offset: (this.currentPage - 1) * this.limit,
                    limit: this.limit
                },
                (data) => {
                    this.entries = data.items;
                    this.totalCount = data.totalCount;
                }
                );
        }

    } // end of class
} // end of module

var app = angular.module('app', ['AppUtilsNg', 'ui.bootstrap', 'chieffancypants.loadingBar']);
app.controller('Ctrl', App.Account_Balance.Ctrl);
