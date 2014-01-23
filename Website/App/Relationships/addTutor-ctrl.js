var App;
(function (App) {
    (function (Relationships_Index) {
        var AddTutorCtrl = (function () {
            function AddTutorCtrl($scope, $http) {
                this.$scope = $scope;
                this.$http = $http;
                this.tutors = [];
                $scope.vm = this;
                this.session = new Date().getTime() % 100;
                this.currentBacket = 0;
                this.getTutors();
            }
            AddTutorCtrl.prototype.getTutors = function () {
                var _this = this;
                var url = App.Utils.relationshipsApiUrl('RandomTutors/' + this.session + '/' + this.currentBacket);
                App.Utils.ngHttpGet(this.$http, url, function (data) {
                    _this.tutors = data;
                });
            };

            AddTutorCtrl.prototype.addTutor = function (tutor) {
                var url = App.Utils.relationshipsApiUrl(tutor.id.toString());
                this.$http.put(url, null, { headers: App.Utils.getSecurityHeader() }).success(function () {
                    toastr.success('The tutor has been added to your tutor list');
                }).error(function (data, status) {
                    if (status === 400 && data.message) {
                        // data.message expected 'Relation already exists'.
                        toastr.info(data.message);
                    } else
                        App.Utils.logNgHttpError(data, status);
                });
            };
            AddTutorCtrl.$inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];
            return AddTutorCtrl;
        })();
        Relationships_Index.AddTutorCtrl = AddTutorCtrl;
    })(App.Relationships_Index || (App.Relationships_Index = {}));
    var Relationships_Index = App.Relationships_Index;
})(App || (App = {}));
//# sourceMappingURL=addTutor-ctrl.js.map
