module app.account {

    export class Login {

        userName: string;
        password: string;
        persistent: boolean = true;
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
                // returnUrl is passed by the server through the page.
                var returnUrl: string = app['returnUrl'] || '/';

                app.ngHttpPost(this.$http,
                    app.accountsApiUrl('login'),
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

    angular.module(app.myAppName, ['angular-loading-bar'])
        .controller('Login', Login);

} // end of module

