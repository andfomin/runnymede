module App.Account_Forgot {

    export class Ctrl {

        email: string;
        sending: boolean;
        done: boolean;

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
                    Utils.accountApiUrl('Forgot'),
                    {
                        email: this.email,
                    },                    
                    () => {
                        this.done = true;
                    },
                    () => {
                        this.sending = false;
                    });
            }
        }

    } // end of class
} // end of module

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Forgot.Ctrl);
