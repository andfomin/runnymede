module App.Relationships_Tutors {

    export class ListCtrl {

        tutors: App.Model.IUser[] = [];

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
                $scope.vm = this;
                this.getTutors();
        }

        private getTutors() {
            var url = App.Utils.relationshipsApiUrl('Tutors');
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.tutors = data;
            });
        }

        private removeTutor(tutor: App.Model.IUser) {
            var url = App.Utils.relationshipsApiUrl('Tutors/' + tutor.id.toString());
            this.$http.delete(url, { headers: App.Utils.getSecurityHeader() })
                .success(() => {
                    toastr.success('The tutor has been removed from your tutor list');
                    var index = this.tutors.indexOf(tutor);
                    this.tutors.splice(index, 1);
                })
                .error(App.Utils.logNgHttpError);
        }

        private payTutor(tutor: App.Model.IUser) {
            window.location.assign(App.Utils.accountUrl('pay-tutor/' + tutor.id.toString() + '?tutor=' + tutor.displayName)); 
        }
    }
}