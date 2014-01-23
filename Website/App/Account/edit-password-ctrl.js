var App;
(function (App) {
    (function (Account_Edit) {
        var PasswordCtrl = (function () {
            function PasswordCtrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                $scope.vm = this;
            }
            PasswordCtrl.prototype.save = function (form) {
                var _this = this;
                if (form.$valid) {
                    this.sending = true;

                    App.Utils.ngHttpPost(this.$http, App.Utils.accountApiUrl('Password'), {
                        oldPassword: this.oldPassword,
                        newPassword1: this.newPassword1,
                        newPassword2: this.newPassword2
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
            PasswordCtrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return PasswordCtrl;
        })();
        Account_Edit.PasswordCtrl = PasswordCtrl;
    })(App.Account_Edit || (App.Account_Edit = {}));
    var Account_Edit = App.Account_Edit;
})(App || (App = {}));
//# sourceMappingURL=edit-password-ctrl.js.map
