var App;
(function (App) {
    (function (Account_Create) {
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

                    // We do not use ngHttpPost() since our finallyCallback execution path is splitted for signIn()
                    this.$http.post(App.Utils.accountApiUrl('Create'), {
                        userName: this.userName,
                        password: this.password,
                        displayName: this.displayName,
                        consent: this.consent
                    }).success(function (data) {
                        // Sign in the user after signup automatically.
                        App.Utils.signIn(_this.$http, _this.userName, _this.password, false, function () {
                            _this.sending = false;
                            _this.done = true;
                        });
                    }).error(function (data, status) {
                        _this.sending = false;
                        App.Utils.logNgHttpError(data, status);
                    });
                }
            };
            Ctrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return Ctrl;
        })();
        Account_Create.Ctrl = Ctrl;
    })(App.Account_Create || (App.Account_Create = {}));
    var Account_Create = App.Account_Create;
})(App || (App = {}));

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Create.Ctrl);
//# sourceMappingURL=create-app.js.map
