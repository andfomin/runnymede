var App;
(function (App) {
    (function (Statistics) {
        var SelectOption = (function () {
            function SelectOption(data) {
                this.createTime = App.Utils.parseDate(data.createTime);
                this.title = App.Utils.formatDateLocal(data.createTime) + ' - ' + (data.title || '');
            }
            return SelectOption;
        })();

        var ViewModel = (function () {
            function ViewModel() {
                var _this = this;
                this.firstFrom = ko.observable();
                this.firstTo = ko.observable();
                this.secondFrom = ko.observable();
                this.secondTo = ko.observable();

                this.firstStats = ko.observableArray();
                this.secondStats = ko.observableArray();

                var exercisesParam = (App).exercisesParam || [];

                this.firstFromArr = ko.observableArray($.map(exercisesParam, function (i) {
                    return new SelectOption(i);
                }));
                this.secondFromArr = ko.observableArray($.map(exercisesParam, function (i) {
                    return new SelectOption(i);
                }));

                this.firstToArr = ko.computed(function () {
                    return _this.firstFrom() ? ko.utils.arrayFilter(_this.firstFromArr() || [], function (i) {
                        return i.createTime >= _this.firstFrom().createTime;
                    }) : null;
                });

                this.secondToArr = ko.computed(function () {
                    return _this.secondFrom() ? ko.utils.arrayFilter(_this.secondFromArr() || [], function (i) {
                        return i.createTime >= _this.secondFrom().createTime;
                    }) : null;
                });

                this.firstFrom.subscribe(function () {
                    _this.firstTo(null);
                });

                this.firstTo.subscribe(function () {
                    _this.firstStats(null);
                });

                this.secondFrom.subscribe(function () {
                    _this.secondTo(null);
                });

                this.secondTo.subscribe(function () {
                    _this.secondStats(null);
                });

                this.loadStats = function (from, to, stats, complete) {
                    App.Utils.activityIndicator(true);

                    var wireFormat = "YYYY-MM-DDTHH:mm:ss";
                    var formattedFrom = moment.utc(from.createTime).format(wireFormat);
                    var formattedTo = moment.utc(to.createTime).format(wireFormat);

                    App.Utils.ajaxRequest('GET', App.Utils.statisticsApiUrl('?from=' + formattedFrom + '&to=' + formattedTo)).done(function (data) {
                        var remarks = $.map(data || [], function (i) {
                            return new App.Model.Remark({ tags: i });
                        });
                        stats(remarks);
                    }).fail(function () {
                        toastr.error('Error getting statistics.');
                    }).always(function () {
                        App.Utils.activityIndicator(false);
                        complete();
                    });
                };

                this.displayFirstCmd = ko.asyncCommand({
                    execute: function (complete) {
                        _this.loadStats(_this.firstFrom(), _this.firstTo(), _this.firstStats, complete);
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting && _this.firstTo();
                    }
                });

                this.displaySecondCmd = ko.asyncCommand({
                    execute: function (complete) {
                        _this.loadStats(_this.secondFrom(), _this.secondTo(), _this.secondStats, complete);
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting && _this.secondTo();
                    }
                });
            }
            return ViewModel;
        })();
        Statistics.ViewModel = ViewModel;
    })(App.Statistics || (App.Statistics = {}));
    var Statistics = App.Statistics;
})(App || (App = {}));

$(function () {
    var vm = new App.Statistics.ViewModel();
    ko.applyBindings(vm);
});
//# sourceMappingURL=index-vm.js.map
