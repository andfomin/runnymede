module App.Account_Edit {

    export class PasswordCtrl {

        oldPassword: string;
        newPassword1: string;
        newPassword2: string;

        sending: boolean;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;

        } // end of ctor
        
        save(form) {
            if (form.$valid) {
                this.sending = true;

                App.Utils.ngHttpPost(this.$http,
                    Utils.accountApiUrl('Password'),
                    {
                        oldPassword: this.oldPassword,
                        newPassword1: this.newPassword1,
                        newPassword2: this.newPassword2,
                    },
                    () => {
                        // The controller signs out and cleans the authorization cookie.
                        window.localStorage.removeItem('accessToken');
                        window.sessionStorage.removeItem('accessToken');
                        window.location.assign('/account/signin?password-changed');                        
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }


    } // end of class
} // end of module
