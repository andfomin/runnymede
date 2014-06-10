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
            }
            return ViewModel;
        })();
        Statistics.ViewModel = ViewModel;
    })(App.Statistics || (App.Statistics = {}));
    var Statistics = App.Statistics;
})(App || (App = {}));

$(function () {
    var vm = new App.Statistics.ViewModel();
});
//# sourceMappingURL=index-vm.js.map
