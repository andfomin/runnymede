var App;
(function (App) {
    (function (Relationships_Index) {
        var TutorsCtrl = (function () {
            function TutorsCtrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                this.tutors = [];
                $scope.vm = this;
                this.getTutors();
            }
            TutorsCtrl.prototype.getTutors = function () {
                var _this = this;
                var url = App.Utils.relationshipsApiUrl('FavoriteTutors');
                App.Utils.ngHttpGet(this.$http, url, function (data) {
                    _this.tutors = data;
                });
            };

            TutorsCtrl.prototype.removeTutor = function (tutor) {
                var _this = this;
                var url = App.Utils.relationshipsApiUrl(tutor.id.toString());
                this.$http.delete(url, { headers: App.Utils.getSecurityHeader() }).success(function () {
                    toastr.success('The tutor has been removed from your tutor list');
                    var index = _this.tutors.indexOf(tutor);
                    _this.tutors.splice(index, 1);
                }).error(App.Utils.logNgHttpError);
            };
            TutorsCtrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return TutorsCtrl;
        })();
        Relationships_Index.TutorsCtrl = TutorsCtrl;
    })(App.Relationships_Index || (App.Relationships_Index = {}));
    var Relationships_Index = App.Relationships_Index;
})(App || (App = {}));
//# sourceMappingURL=tutors-ctrl.js.map
