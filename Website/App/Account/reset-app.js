var App;
(function (App) {
    (function (Account_Reset) {
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

                    App.Utils.ngHttpPost(this.$http, App.Utils.accountApiUrl('Reset' + window.location.search), {
                        password1: this.password1,
                        password2: this.password2
                    }, function () {
                        // The controller signs out and cleans the authorization cookie.
                        window.localStorage.removeItem('accessToken');
                        window.sessionStorage.removeItem('accessToken');
                        window.location.assign('/account/signin?password-changed');
                    }, function () {
                        _this.sending = false;
                    });
                }
            };
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Account_Reset.Ctrl = Ctrl;
    })(App.Account_Reset || (App.Account_Reset = {}));
    var Account_Reset = App.Account_Reset;
})(App || (App = {}));

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Reset.Ctrl);
//# sourceMappingURL=reset-app.js.map
