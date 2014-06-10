module App.Teachers_Index {

    export class Favorites {

        teachers: App.Model.IUser[] = null;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.getTeachers();
        }

        noTeachers = () => {
            return (this.teachers != null) && (this.teachers.length === 0);
        }

        private getTeachers() {
            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.teachersApiUrl('Teachers'),
                null,
                (data) => {
                    this.teachers = data;
                });
        }

        private removeTeacher(teacher: App.Model.IUser) {
            this.$http.delete(App.Utils.teachersApiUrl('Teachers/' + teacher.id))
                .success(() => {
                    toastr.success('The teacher has been removed from your favorite teacher list');
                    App.Utils.arrRemove(this.teachers, teacher);
                })
                .error(App.Utils.logError);
        }

        private payTeacher(teacher: App.Model.IUser) {
            window.location.assign(App.Utils.accountUrl('pay-teacher/' + teacher.id));
        }
    }
}