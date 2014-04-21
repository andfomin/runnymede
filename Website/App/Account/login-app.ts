module App.Account_Login {

    export class Ctrl {

        userName: string;
        password: string;
        persistent: boolean = true;
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
                // returnUrl is passed by the server through the page.
                var returnUrl: string = App['returnUrl'] || '/';

                App.Utils.ngHttpPost(this.$http,
                    App.Utils.accountApiUrl('Login'),
                    {
                        userName: this.userName,
                        password: this.password,
                        persistent: this.persistent
                    },
                    () => { window.location.replace(returnUrl); },
                    () => { this.sending = false; }
                    );
            }
        }
    } // end of class
} // end of module

var app = angular.module("app", ['chieffancypants.loadingBar']);
app.controller("Ctrl", App.Account_Login.Ctrl);
