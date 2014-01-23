var App;
(function (App) {
    (function (Account_Balance) {
        var BalanceEntry = (function () {
            function BalanceEntry(data) {
                this.observedTime = App.Utils.formatDateLocal(data.observedTime);
                this.description = data.description || '';
                this.debit = App.Utils.formatMoney(data.debit);
                this.credit = App.Utils.formatMoney(data.credit);
                this.balance = App.Utils.formatMoney(data.balance);
            }
            return BalanceEntry;
        })();
        Account_Balance.BalanceEntry = BalanceEntry;

        var ViewModel = (function () {
            function ViewModel() {
                this.entries = ko.observableArray([]).extend({
                    datasource: App.Utils.dataSource(App.Utils.balanceApiUrl(), function (i) {
                        return new BalanceEntry(i);
                    }),
                    pager: {
                        limit: 10
                    }
                });
            }
            return ViewModel;
        })();
        Account_Balance.ViewModel = ViewModel;
    })(App.Account_Balance || (App.Account_Balance = {}));
    var Account_Balance = App.Account_Balance;
})(App || (App = {}));

$(function () {
    var vm = new App.Account_Balance.ViewModel();
    ko.applyBindings(vm);
});
//# sourceMappingURL=balance-vm.js.map
