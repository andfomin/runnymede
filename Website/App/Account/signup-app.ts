module App.Account_Signup {

    export class Password {

        email: string;
        password: string;
        name: string;
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
                this.$http.post(
                    App.Utils.accountApiUrl('Signup'),
                    {
                        email: this.email,
                        password: this.password,
                        name: this.name
                    }
                    )
                    .success(() => {
                        // The user is logged in during signup automatically.
                        window.location.assign('/');
                    })
                    .error((data, status) => {
                        this.sending = false;
                        App.Utils.logError(data, status);
                    });
            }
        }

    } // end of class
} // end of module

var app = angular.module('app', ['chieffancypants.loadingBar']);
app.controller('Password', App.Account_Signup.Password);
