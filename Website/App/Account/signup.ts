module app.account {

    export class Signup {

        email: string;
        password: string;
        showPwd: boolean;
        name: string;
        sending: boolean;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$timeout];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService
            ) {
            $scope.vm = this;
        } // end of ctor

        post(form) {
            if (form.$valid) {
                this.sending = true;
                app.ngHttpPost(this.$http,
                    app.accountsApiUrl('signup'),
                    {
                        email: this.email,
                        password: this.password,
                        name: this.name
                    },
                    (data) => {
                        // The user is logged in during signup automatically.
                        // Please check your email and click the link provided to confirm your registration.
                        window.location.assign(data);
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }

        focusPwd = () => {
            var id = 'password' + (this.showPwd ? '2' : '1');
            var input = document.getElementById(id);
            this.$timeout(() => {
                input.focus();
            }, 100);
        };

    } // end of class

    angular.module(app.myAppName, ['angular-loading-bar'])
        .controller('Signup', Signup);

} // end of module

