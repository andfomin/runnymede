var App;
(function (App) {
    (function (Account_Edit) {
        var ProfileCtrl = (function () {
            function ProfileCtrl($scope, $http) {
                var _this = this;
                this.$scope = $scope;
                this.$http = $http;
                this.load = function () {
                    App.Utils.ngHttpGet(_this.$http, App.Utils.accountApiUrl('Profile'), function (data) {
                        _this.profile = data;
                    });
                };
                $scope.vm = this;
                this.load();
            }
            ProfileCtrl.prototype.save = function (form) {
                var _this = this;
                if (form.$valid) {
                    this.sending = true;
                    this.displayNameChanged = false;
                    this.skypeChanged = false;
                    var displayNameDirty = (form).displayName.$dirty ? true : false;
                    var skypeDirty = (form).skype.$dirty ? true : false;
                    var rateARecDirty = (form).rateARec.$dirty ? true : false;

                    App.Utils.ngHttpPut(this.$http, App.Utils.accountApiUrl('Profile'), {
                        displayName: displayNameDirty ? this.profile.displayName : null,
                        skype: skypeDirty ? this.profile.skype : null,
                        rateARec: rateARecDirty ? this.profile.rateARec : null
                    }, function () {
                        form.$setPristine();
                        _this.displayNameChanged = displayNameDirty;
                        _this.skypeChanged = skypeDirty;
                        _this.rateARecChanged = rateARecDirty;
                    }, function () {
                        _this.sending = false;
                    });
                }
            };
            ProfileCtrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return ProfileCtrl;
        })();
        Account_Edit.ProfileCtrl = ProfileCtrl;
    })(App.Account_Edit || (App.Account_Edit = {}));
    var Account_Edit = App.Account_Edit;
})(App || (App = {}));
//# sourceMappingURL=edit-profile-ctrl.js.map
