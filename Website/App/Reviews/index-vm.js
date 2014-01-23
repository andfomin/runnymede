var App;
(function (App) {
    (function (Reviews_Index) {
        var ViewModel = (function () {
            function ViewModel() {
                this.reviews = ko.observableArray([]).extend({
                    datasource: App.Utils.dataSource(App.Utils.reviewsApiUrl() + App.Utils.getNoCacheUrl(), function (i) {
                        return new App.Model.Review(i);
                    }),
                    pager: {
                        limit: 10
                    }
                });
            }
            return ViewModel;
        })();
        Reviews_Index.ViewModel = ViewModel;
    })(App.Reviews_Index || (App.Reviews_Index = {}));
    var Reviews_Index = App.Reviews_Index;
})(App || (App = {}));

$(function () {
    var vm = new App.Reviews_Index.ViewModel();
    ko.applyBindings(vm);
});
//# sourceMappingURL=index-vm.js.map
