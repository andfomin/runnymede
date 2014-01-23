module App.Relationships_Index {

    export class AddTutorCtrl {

        tutors: App.Model.ITutor[] = [];
        private session: number;
        private currentBacket: number;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.session = new Date().getTime() % 100; // The range can be such small since it is anyway normalized by sin() in SQL.
            this.currentBacket = 0;
            this.getTutors();
        }

        private getTutors() {
            var url = App.Utils.relationshipsApiUrl('RandomTutors/' + this.session + '/' + this.currentBacket);
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.tutors = data;
            });
        }

        private addTutor(tutor: App.Model.IUser) {
            var url = App.Utils.relationshipsApiUrl(tutor.id.toString());
            this.$http.put(url, null, { headers: App.Utils.getSecurityHeader() })
                .success(() => {
                    toastr.success('The tutor has been added to your tutor list');
                })
                .error((data, status) => {
                    if (status === 400 && data.message) {
                        // data.message expected 'Relation already exists'.
                        toastr.info(data.message);
                    }
                    else App.Utils.logNgHttpError(data, status);
            });
        }
    }
}