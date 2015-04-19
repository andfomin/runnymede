module app.account {

    export class TransactionsReviews extends app.CtrlBase {

        entries: any[];
        balance: number;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            super($scope);
            this.pgLimit = app.pgLimit * 2;
            this.pgLoad();
        } // end of ctor

        pgLoad = () => {
            app.ngHttpGet(this.$http,
                app.accountsApiUrl('review_transactions'),
                {
                    offset: this.pgOffset(),
                    limit: this.pgLimit
                },
                (data) => {
                    this.entries = data.items;
                    this.pgTotal = data.totalCount;
                    // "null" is not sent by the JSON serializer, so it becomes "undefined" on the client side.                     
                    this.balance = app.isNumber(data.balance) ? data.balance : null; 
                }
                );
        }

    } // end of class Ctrl

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('TransactionsReviews', TransactionsReviews);

} // end of module

