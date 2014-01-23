var App;
(function (App) {
    (function (_) {
        var Ctrl = (function () {
            function Ctrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                $scope.vm = this;
            }
            Ctrl.$inject = ['$scope', '$http'];
            return Ctrl;
        })();
        _.Ctrl = Ctrl;
    })(App._ || (App._ = {}));
    var _ = App._;
})(App || (App = {}));

var app = angular.module('app', []);
app.controller('Ctrl', App._.Ctrl);
//# sourceMappingURL=ctrl-template.js.map
