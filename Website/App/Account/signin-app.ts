module App.Account_Signin {

    export class Ctrl {

        userName: string;
        password: string;
        persistent: boolean = false;
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
                // returnUrl is passed by the controller through the page.
                var redirectTo: string = App['returnUrl'] || '/';

                App.Utils.signIn(this.$http, this.userName, this.password, this.persistent,
                    () => {
                        this.sending = false;
                    },
                    redirectTo
                    );
            }
        }
    } // end of class
} // end of module

var app = angular.module("app", ['chieffancypants.loadingBar']);
app.controller("Ctrl", App.Account_Signin.Ctrl);
