module App.Account_Reset {

    export class Ctrl {

        password1: string;
        password2: string;
        sending: boolean;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

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
                        password1: this.password1,
                        password2: this.password2,
                    },
                    () => {
                        // The controller signs out and cleans the authorization cookie.
                        window.localStorage.removeItem('accessToken');
                        window.sessionStorage.removeItem('accessToken');
                        window.location.assign('/account/signin?password-changed');                        
                    },
                    () => {
                        this.sending = false;
                    });
            }
        }

    } // end of class
} // end of module

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Reset.Ctrl);
