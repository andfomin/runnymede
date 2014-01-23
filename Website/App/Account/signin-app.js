var App;
(function (App) {
    (function (Account_Signin) {
        var Ctrl = (function () {
            function Ctrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                this.persistent = false;
                $scope.vm = this;
            }
            Ctrl.prototype.post = function (form) {
                var _this = this;
                if (form.$valid) {
                    this.sending = true;

                    // returnUrl is passed by the controller through the page.
                    var redirectTo = App['returnUrl'] || '/';

                    App.Utils.signIn(this.$http, this.userName, this.password, this.persistent, function () {
                        _this.sending = false;
                    }, redirectTo);
                }
            };
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Account_Signin.Ctrl = Ctrl;
    })(App.Account_Signin || (App.Account_Signin = {}));
    var Account_Signin = App.Account_Signin;
})(App || (App = {}));

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Signin.Ctrl);
//# sourceMappingURL=signin-app.js.map
