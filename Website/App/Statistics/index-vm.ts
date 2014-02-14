interface App {
    // Passed via a script in the HTML
    exercisesParam: any;
}

module App.Statistics {

    export interface ISelectOption {
        createTime: Date;
        title: string;
    }

    class SelectOption implements ISelectOption {
        createTime: Date;
        title: string;

        constructor(data: any) {
            this.createTime = App.Utils.parseDate(data.createTime);
            this.title = App.Utils.formatDateLocal(data.createTime) + ' - ' + (data.title || '');
        } // end of ctor
    } // end of class

    export class ViewModel {

        firstFrom: KnockoutObservable<ISelectOption>;
        firstTo: KnockoutObservable<ISelectOption>;
        secondFrom: KnockoutObservable<ISelectOption>;
        secondTo: KnockoutObservable<ISelectOption>;

        firstFromArr: KnockoutObservableArray<ISelectOption>;
        firstToArr: KnockoutComputed<ISelectOption[]>;
        secondFromArr: KnockoutObservableArray<ISelectOption>;
        secondToArr: KnockoutComputed<ISelectOption[]>;

        displayFirstCmd: KoliteCommand;
        displaySecondCmd: KoliteCommand;

        firstStats: KnockoutObservableArray<App.Model.Remark>;
        secondStats: KnockoutObservableArray<App.Model.Remark>;

        private loadStats: (from: ISelectOption, to: ISelectOption, stats: KnockoutObservableArray<App.Model.Remark>, complete: any) => any;

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
                App.Utils.activityIndicator(true);

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
                        App.Utils.activityIndicator(false);
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
