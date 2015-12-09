module app.account {

    export interface IPricelistItem {
        title: string;
        price: number;
        quantity: number;
    };

    export class BuyServices {

        pricelist: IPricelistItem[];
        busy: boolean = false;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            $scope.vm = this;

            this.pricelist = app['pricelistParam'];

        } // end of ctor

        getTotal = () => {
            var st = this.pricelist.reduce((previous: number, value: IPricelistItem) => {
                return previous + value.price * (value.quantity || 0);
            },
                0);
            return app.numberToMoney(st);
        };

        //calcDiscount = () => {
        //    var st = this.calcSubtotal();
        //    var d = app.arrFind(this.pricelist.discounts,(i) => { return (st >= i.amountFrom) && (st <= i.amountTo); });
        //    var percent = (d && d.percent) || 0;
        //    return app.numberToMoney(st * percent / 100);            
        //};

        canSubmit = () => {
            return (this.getTotal() != 0) && !this.busy && app.isAuthenticated(); 
        };

        onSubmit = () => {
            this.busy = true;
        };

    } // end of class BuyReviews

    angular.module(app.myAppName, [app.utilsNg])
        .controller('BuyServices', BuyServices);

} // end of module

