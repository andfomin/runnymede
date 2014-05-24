module App.Reviews_Index {

    export class Ctrl {

        reviews: App.Model.IReview2[] = [];
        limit: number = 10; // items per page
        currentPage: number = 1;
        totalCount: number = 0;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$interval];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            $interval: ng.IIntervalService
            ) {
            $scope.vm = this;
            this.getReviews();
            $interval(() => { this.getReviews(); }, 300000);
        } // end of ctor

        getReviews() {
            App.Utils.ngHttpGetWithParamsNoCache(this.$http,
                App.Utils.reviewsApiUrl(),
                {
                    offset: (this.currentPage - 1) * this.limit,
                    limit: this.limit
                },
                (data) => {
                    this.reviews = data.items;
                    this.totalCount = data.totalCount;
                }
                );
        }

        isEmpty() {
            return !this.reviews || this.reviews.length == 0;
        }



    } // end of class
} // end of module

var app = angular.module('app', ['AppUtilsNg', 'ui.bootstrap', 'chieffancypants.loadingBar']);
app.controller('Ctrl', App.Reviews_Index.Ctrl);
