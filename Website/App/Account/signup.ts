module app.account {

    export class Signup {

        email: string = null;
        password: string = null;
        name: string;
        showPwd: boolean;
        sending: boolean;
        submited: boolean = false;
        minPwdLength = 6;
        // +https://github.com/angular/angular.js/blob/master/src/ng/directive/input.js
        EMAIL_REGEXP = /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$timeout];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService
            ) {
            $scope.vm = this;
        } // end of ctor

        post(form) {
            this.submited = true;
            if (this.isEmailValid() && this.isPwdValid()) {
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
                        // Redirect to the greeting page.
                        window.location.assign(data);
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
            else {
                toastr.warning('Please enter correct information.');
            }
        }

        isEmailValid = () => {
            var ok = (this.email !== null) && this.EMAIL_REGEXP.test(this.email);
            return !this.submited || ok;
        };

        isPwdValid = () => {
            var ok = (this.password !== null) && (this.password.length >= this.minPwdLength);
            return !this.submited || ok;
        };

        focusPwd = () => {
            var id = 'password' + (this.showPwd ? '2' : '1');
            var input = document.getElementById(id);
            this.$timeout(() => {
                input.focus();
            }, 100);
        };

    } // end of class Signup

    angular.module(app.myAppName, ['angular-loading-bar'])
        .controller('Signup', Signup);

} // end of module

