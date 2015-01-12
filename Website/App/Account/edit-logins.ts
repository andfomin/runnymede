module app.account_edit {

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
        loginsLoaded: boolean = false;

        profile: app.IUser;
        newEmail: string;
        emailConfirmed: boolean;
        linkSent: boolean = false;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$stateParams, app.ngNames.$location];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $stateParams: ng.ui.IStateParamsService,
            $location: ng.ILocationService
            ) {
            $scope.vm = this;

            // An error may be returned by AccountController.ExternalLoginCallback after an external login attempt.
            var error = $stateParams['error']; // Otherwise decodeURIComponent returns 'undefined'.
            if (error) {
                this.error = decodeURIComponent(error);
            }
            // Clear the error text in the address bar. We have specified reloadOnSearch: false in ng.ui.IState
            $location.search('error', null); 

            var self = app.getSelfUser();
            this.isTeacher = self.isTeacher;
            this.userName = self.userName;

            this.loadProfile();
            this.loadLogins();
        } // end of ctor

        private loadProfile() {
            app.ngHttpGet(this.$http,
                app.accountsApiUrl('personal_profile'),
                null,
                (data) => {
                    this.profile = data;
                    this.newEmail = this.profile.email;
                });
        }

        private loadLogins() {
            app.ngHttpGet(this.$http,
                app.accountsApiUrl('logins'),
                null,
                (data) => {
                    this.userLogins = data.userLogins;
                    this.otherLogins = data.otherLogins;
                    this.hasPassword = data.hasPassword;
                    this.loginsLoaded = true;
                });
        }

        canRemove() {
            return this.hasPassword || this.userLogins.length > 1;
        }

        removeLogin(loginProvider, providerKey) {
            this.sending = true;
            this.$http.delete(app.accountsApiUrl('external_login?loginProvider=' + loginProvider + '&providerKey=' + providerKey))
                .success(() => { this.loadLogins(); })
                .error(app.logError)
                .finally(() => { this.sending = false; });
        }

        savePassword(form) {
            if (form.$valid) {
                this.sending = true;

                app.ngHttpPut(this.$http,
                    app.accountsApiUrl('password'),
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

        sendEmailLink() {
            this.sending = true;
            app.ngHttpPut(this.$http,
                app.accountsApiUrl('email'),
                {
                    email: this.newEmail
                },
                () => {
                    this.linkSent = true;
                },
                () => { this.sending = false; }
                );
        }

    } // end of class
} // end of module
