module app.account {

    export class ForgotPassword {

        email: string;
        sending: boolean;
        done: boolean;

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
                    app.accountsApiUrl('forgot'),
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

    angular.module(app.myAppName, ['angular-loading-bar'])
        .controller('ForgotPassword', ForgotPassword);

} // end of module

