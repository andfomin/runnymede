module app.account{

    export class ResetPassword  {

        password: string;
        sending: boolean;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

        post(form) {
            if (form.$valid) {
                this.sending = true;

                app.ngHttpPost(this.$http,
                    app.accountsApiUrl('reset' + window.location.search),
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

    angular.module(app.myAppName, ['angular-loading-bar'])
        .controller("ResetPassword", ResetPassword);

} // end of module

