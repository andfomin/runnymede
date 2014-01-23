module App.Account_Create {

    export class Ctrl {

        userName: string;
        password: string;
        displayName: string;
        consent: boolean;
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

                // We do not use ngHttpPost() since our finallyCallback execution path is splitted for signIn()
                this.$http.post(
                    Utils.accountApiUrl('Create'),
                    {
                        userName: this.userName,
                        password: this.password,
                        displayName: this.displayName,
                        consent: this.consent,
                    }
                    )
                    .success((data) => {
                        // Sign in the user after signup automatically.
                        App.Utils.signIn(this.$http, this.userName, this.password, false,
                            () => {
                                this.sending = false;
                                this.done = true;
                            });
                    })
                    .error((data, status) => {
                        this.sending = false;
                        App.Utils.logNgHttpError(data, status);
                    });
            }
        }

    } // end of class
} // end of module

var app = angular.module("app", []);
app.controller("Ctrl", App.Account_Create.Ctrl);
