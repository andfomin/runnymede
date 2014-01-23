var App;
(function (App) {
    (function (Reviews_Requests) {
        var Ctrl = (function () {
            function Ctrl($scope, $http) {
                var _this = this;
                this.$scope = $scope;
                this.$http = $http;
                this.requests = [];
                this.refresh = function () {
                    App.Utils.ngHttpGet(_this.$http, App.Utils.reviewsApiUrl('Requests'), function (data) {
                        _this.requests = data;
                    });
                };
                this.start = function () {
                    var request = _this.dialogRequest;
                    _this.dialogRequest = null;

                    if (request) {
                        App.Utils.ngHttpPost(_this.$http, App.Utils.reviewsApiUrl(request.reviewId.toString() + '/Start'), null, function () {
                            window.location.assign(App.Utils.reviewsUrl('edit/' + request.reviewId.toString()));
                        }, function () {
                            ($('#startDialog')).modal('hide');
                        });
                    }
                };
                this.showStartDialog = function (request) {
                    _this.dialogRequest = request;
                    ($('#startDialog')).modal();
                };
                this.formatMsec = function (length) {
                    return App.Utils.formatMsec(length);
                };
                this.calcRate = function (length, reward) {
                    return App.Utils.numberToMoney(reward * 60000 / length);
                };
                $scope.vm = this;

                this.refresh();
                window.setInterval(this.refresh, 30000);
            }
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Reviews_Requests.Ctrl = Ctrl;
    })(App.Reviews_Requests || (App.Reviews_Requests = {}));
    var Reviews_Requests = App.Reviews_Requests;
})(App || (App = {}));

var app = angular.module('app', []);
app.controller('Ctrl', App.Reviews_Requests.Ctrl);
//# sourceMappingURL=requests-ctrl.js.map
