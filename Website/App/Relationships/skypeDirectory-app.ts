module App.Relationships_SkypeDirectory {

    export class Ctrl {

        learners: any[] = [];

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.getDirectory();
        }

        private getDirectory = () => {
            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.relationshipsApiUrl('SkypeDirectory'),
                null,
                (data) => { this.learners = data; });
        }

        private showRemoveDialog = () => {
            (<any>$('#removeDialog')).modal();
        }

        private remove = () => {
            this.$http.delete(App.Utils.relationshipsApiUrl('SkypeDirectory'))
                .finally(() => window.location.assign(App.Utils.relationshipsUrl('skype-directory/join')));
        }

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Relationships_SkypeDirectory.Ctrl);

app.config([
    '$compileProvider',
    function ($compileProvider) {
        $compileProvider.aHrefSanitizationWhitelist(/^skype:/);
    }
]);
