var App;
(function (App) {
    (function (Account_Forgot) {
        var Ctrl = (function () {
            function Ctrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                $scope.vm = this;
            }
            Ctrl.prototype.post = function (form) {
                var _this = this;
                if (form.$valid) {
                    this.sending = true;

                    App.Utils.ngHttpPost(this.$http, App.Utils.accountApiUrl('Forgot'), {
                        email: this.email
                    }, function () {
                        _this.done = true;
                    }, function () {
                        _this.sending = false;
                    });
                }
            };
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Account_Forgot.Ctrl = Ctrl;
    })(App.Account_Forgot || (App.Account_Forgot = {}));
    var Account_Forgot = App.Account_Forgot;
})(App || (App = {}));

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Forgot.Ctrl);
//# sourceMappingURL=forgot-app.js.map
