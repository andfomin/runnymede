module App.Relationships_TutorLearners {

    export class Ctrl {

        learners: App.Model.IUser[] = [];

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.getLearners();
        }

        private getLearners() {
            var url = App.Utils.relationshipsApiUrl('TutorLearners');
            App.Utils.ngHttpGet(this.$http, url, (data) => {
                this.learners = data;
            });
        }

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Relationships_TutorLearners.Ctrl);
