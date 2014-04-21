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
            var url = App.Utils.teachersApiUrl('Teachers');
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.teachers = data;
            });
        }

        private removeTeacher(teacher: App.Model.IUser) {
            var url = App.Utils.teachersApiUrl('Teachers/' + teacher.id.toString());
            this.$http.delete(url)
                .success(() => {
                    toastr.success('The teacher has been removed from your teacher list');
                    var index = this.teachers.indexOf(teacher);
                    this.teachers.splice(index, 1);
                })
                .error(App.Utils.logError);
        }

        private payTeacher(teacher: App.Model.IUser) {
            window.location.assign(App.Utils.accountUrl('pay-teacher/' + teacher.id.toString() + '?teacher=' + teacher.displayName)); 
        }
    }
}