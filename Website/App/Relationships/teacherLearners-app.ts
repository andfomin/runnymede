module App.Relationships_TeacherLearners {

    export class Ctrl {

        learners: App.Model.IUser[] = [];

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.getLearners();
        }

        private getLearners() {
            var url = App.Utils.relationshipsApiUrl('TeacherLearners');
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.learners = data;
            });
        }

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Relationships_TeacherLearners.Ctrl);
