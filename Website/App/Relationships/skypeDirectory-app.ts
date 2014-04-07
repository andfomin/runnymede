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
            var url = App.Utils.relationshipsApiUrl('SkypeDirectory');
            App.Utils.ngHttpGet(this.$http, url, (data) =>
            {
                this.learners = data;
            });
        }

        private showRemoveDialog = () => {
            (<any>$('#removeDialog')).modal();
        }

        private remove = () => {
            var url = App.Utils.relationshipsApiUrl('SkypeDirectory');
            this.$http.delete(url, { headers: App.Utils.getSecurityHeader() })
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
