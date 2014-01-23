module App.Account_Balance {

    export class BalanceEntry {
        observedTime: string;
        description: string;
        debit: string;
        credit: string;
        balance: string;

        constructor(data: any) {
            this.observedTime = App.Utils.formatDateLocal(data.observedTime);
            this.description = data.description || '';
            this.debit = App.Utils.formatMoney(data.debit);
            this.credit = App.Utils.formatMoney(data.credit);
            this.balance = App.Utils.formatMoney(data.balance);
        } // end of ctor
    } // end of class

    export class ViewModel {

        entries: KnockoutObservableArray<BalanceEntry>;

        constructor() {

            this.entries = ko.observableArray([]).extend({
                datasource: App.Utils.dataSource(
                    App.Utils.balanceApiUrl(),
                    (i) => { return new BalanceEntry(i); }
                    ),
                pager: {
                    limit: 10
                }
            });
        } // end of ctor

    } // end of class
} // end of module

$(() => {
    var vm = new App.Account_Balance.ViewModel();
    ko.applyBindings(vm);
});
