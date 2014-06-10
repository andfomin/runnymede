module App.Account_Edit {

    export class Logins {

        oldPassword: string = null;
        newPassword: string = null;
        isTeacher: boolean;
        userName: string;
        userLogins: any[] = []; // { loginProvider, providerKey }
        otherLogins: string[] = [];
        hasPassword: boolean;
        sending: boolean;
        error: string = null;
        loaded: boolean = false;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$routeParams, App.Utils.ngNames.$location];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            $routeParams: ng.route.IRouteParamsService,
            $location: ng.ILocationService
            ) {
            $scope.vm = this;

            // An error, if any, is returned by the callback action after an external login attempt.
            var error = (<any>$routeParams).error; // Otherwise decodeURIComponent returns 'undefined'.
            if (error) {
                this.error = decodeURIComponent(error);
            }
            $location.search('error', null); // Clear the error text in the address bar. We have specified reloadOnSearch: false in $routeProvider config

            this.isTeacher = (<any>App).isTeacher; // Passed via the page.
            this.userName = (<any>App).userName;

            this.getLogins();
        } // end of ctor

        getLogins() {
            App.Utils.ngHttpGetNoCache(this.$http,
                Utils.accountApiUrl('Logins'),
                null,
                (data) => {
                    this.userLogins = data.userLogins;
                    this.otherLogins = data.otherLogins;
                    this.hasPassword = data.hasPassword;
                    this.loaded = true;
                });
        }

        canRemove() {
            return this.hasPassword || this.userLogins.length > 1;
        }

        removeLogin(loginProvider, providerKey) {
            this.sending = true;
            this.$http.delete(App.Utils.accountApiUrl('ExternalLogin?loginProvider=' + loginProvider + '&providerKey=' + providerKey))
                .success(() => { this.getLogins(); })
                .error(App.Utils.logError)
                .finally(() => { this.sending = false; });
        }

        savePassword(form) {
            if (form.$valid) {
                this.sending = true;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('Password'),
                    {
                        oldPassword: this.oldPassword,
                        newPassword: this.newPassword,
                    },
                    () => {
                        // The controller has signed the user out and cleaned the authorization cookie.
                        window.location.assign('/account/login?password-changed');
                    },
                    () => {
                        this.sending = false;
                        this.oldPassword = null;
                        this.newPassword = null;
                    }
                    );
            }
        }

    } // end of class
} // end of module
