module App.Relationships_SkypeDirectoryJoin {

    export class Ctrl {

        skype: string;
        announcement: string;
        sending: boolean;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.skype = App['skype'] || null;
        } // end of ctor

        post(form) {
            if (form.$valid) {
                this.sending = true;
                App.Utils.ngHttpPost(this.$http,
                    App.Utils.relationshipsApiUrl('SkypeDirectory'),
                    {
                        skype: this.skype,
                        announcement: this.announcement,
                    },
                    null,
                    () => { window.location.assign(App.Utils.relationshipsUrl('skype-directory')); }
                    );
            }
        }

    } // end of class
} // end of module

var app = angular.module("app", []);
app.controller("Ctrl", App.Relationships_SkypeDirectoryJoin.Ctrl);