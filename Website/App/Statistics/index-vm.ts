interface App {
    // Passed via a script in the HTML
    exercisesParam: any;
}

module App.Statistics {

    class SelectOption {
        createTime: Date;
        title: string;

        constructor(data: any) {
            this.createTime = App.Utils.parseDate(data.createTime);
            this.title = App.Utils.formatDateLocal(data.createTime) + ' - ' + (data.title || '');
        } // end of ctor
    } // end of class

    export class ViewModel {

        firstFrom: KnockoutObservable<any>;
        firstTo: KnockoutObservable<any>;
        secondFrom: KnockoutObservable<any>;
        secondTo: KnockoutObservable<any>;

        firstFromArr: KnockoutObservableArray<any>;
        firstToArr: KnockoutComputed<any[]>;
        secondFromArr: KnockoutObservableArray<any>;
        secondToArr: KnockoutComputed<any[]>;

        displayFirstCmd: KoliteCommand;
        displaySecondCmd: KoliteCommand;

        firstStats: KnockoutObservableArray<any>;
        secondStats: KnockoutObservableArray<any>;

        private loadStats: (from: any, to: any, stats: KnockoutObservableArray<App.Model.Remark>, complete: any) => any;

        constructor() {

            this.firstFrom = ko.observable();
            this.firstTo = ko.observable();
            this.secondFrom = ko.observable();
            this.secondTo = ko.observable();

            this.firstStats = ko.observableArray();
            this.secondStats = ko.observableArray();

            var exercisesParam = (<any>App).exercisesParam || [];

            this.firstFromArr = ko.observableArray($.map(exercisesParam, i => new SelectOption(i)));
            this.secondFromArr = ko.observableArray($.map(exercisesParam, i => new SelectOption(i)));

            this.firstToArr = ko.computed(() => {
                return this.firstFrom()
                    ? ko.utils.arrayFilter(this.firstFromArr() || [], i => i.createTime >= this.firstFrom().createTime)
                    : null;
            });

            this.secondToArr = ko.computed(() => {
                return this.secondFrom()
                    ? ko.utils.arrayFilter(this.secondFromArr() || [], i => i.createTime >= this.secondFrom().createTime)
                    : null;
            });

            this.firstFrom.subscribe(() => {
                this.firstTo(null);
            });

            this.firstTo.subscribe(() => {
                this.firstStats(null);
            });

            this.secondFrom.subscribe(() => {
                this.secondTo(null);
            });

            this.secondTo.subscribe(() => {
                this.secondStats(null);
            });

            this.loadStats = (from, to, stats, complete) => {
                var wireFormat = "YYYY-MM-DDTHH:mm:ss";
                var formattedFrom = moment.utc(from.createTime).format(wireFormat);
                var formattedTo = moment.utc(to.createTime).format(wireFormat);

                App.Utils.ajaxRequest('GET',
                    App.Utils.statisticsApiUrl('?from=' + formattedFrom + '&to=' + formattedTo))
                    .done((data) => {
                        var remarks = $.map(data || [], i => {
                            return new App.Model.Remark({ tags: i });
                        });
                        stats(remarks);
                    })
                    .fail(() => { toastr.error('Error getting statistics.'); })
                    .always(() => {
                        complete();
                    });
            };

            this.displayFirstCmd = ko.asyncCommand({
                execute: (complete) => {
                    this.loadStats(this.firstFrom(), this.firstTo(), this.firstStats, complete);
                },
                canExecute: (isExecuting) => {
                    return !isExecuting && this.firstTo();
                }
            });

            this.displaySecondCmd = ko.asyncCommand({
                execute: (complete) => {
                    this.loadStats(this.secondFrom(), this.secondTo(), this.secondStats, complete);
                },
                canExecute: (isExecuting) => {
                    return !isExecuting && this.secondTo();
                }
            });

        } // end of ctor

    } // end of class
}

$(() => {
    var vm = new App.Statistics.ViewModel();
    ko.applyBindings(vm);
});
