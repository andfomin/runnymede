module App.Account_Reset {

    export class Ctrl {

        password: string;
        sending: boolean;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

        post(form) {
            if (form.$valid) {
                this.sending = true;

                App.Utils.ngHttpPost(this.$http,
                    Utils.accountApiUrl('Reset' + window.location.search),
                    {
                        password: this.password,
                    },
                    () => {
                        // The controller signs out and cleans the authorization cookie.
                        window.location.assign('/account/login?password-changed');                        
                    },
                    () => {
                        this.sending = false;
                    });
            }
        }

    } // end of class
} // end of module

var app = angular.module("app", ['chieffancypants.loadingBar']);
app.controller("Ctrl", App.Account_Reset.Ctrl);
