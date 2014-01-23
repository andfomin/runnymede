module App.Relationships_Index {

    export class TutorsCtrl {

        tutors: App.Model.ITutor[] = [];

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
                $scope.vm = this;
                this.getTutors();
        }

        private getTutors() {
            var url = App.Utils.relationshipsApiUrl('FavoriteTutors');
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.tutors = data;
            });
        }

        private removeTutor(tutor: App.Model.ITutor) {
            var url = App.Utils.relationshipsApiUrl(tutor.id.toString());
            this.$http.delete(url, { headers: App.Utils.getSecurityHeader() })
                .success(() => {
                    toastr.success('The tutor has been removed from your tutor list');
                    var index = this.tutors.indexOf(tutor);
                    this.tutors.splice(index, 1);
                })
                .error(App.Utils.logNgHttpError);
        }
    }
}