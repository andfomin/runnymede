module App.Teachers_Index {

    // See +http://stackoverflow.com/questions/14107531/retain-scroll-position-on-route-change-in-angularjs
    export class All {

        teachers: App.Model.IUser[] = [];
        private currentBacket: number;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, 'viewSession'];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private viewSession: number
            ) {
            $scope.vm = this;
            this.currentBacket = 0;
            this.getTeachers();
        }

        private getTeachers() {
            var url = App.Utils.teachersApiUrl('RandomTeachers/' + this.viewSession + '/' + this.currentBacket);
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.teachers = data;
            });
        }

        private addTeacher(teacher: App.Model.IUser) {
            var url = App.Utils.teachersApiUrl('Teachers/' + teacher.id.toString());
            this.$http.put(url, null, { headers: App.Utils.getSecurityHeader() })
                .success(() => {
                    toastr.success('The teacher has been added to your teacher list');
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