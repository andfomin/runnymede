module app.account_edit {

    export class Teacher {

        profile: app.IUser;
        sending: boolean;
        codeSent: boolean = false;
        phoneCode: string;
        newPhone: string;
        phoneToVerify: string;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            $scope.vm = this;
            this.load();
        } // end of ctor
        
        private load = () => {
            app.ngHttpGet(this.$http,
                app.accountsApiUrl('teacher_profile'),
                null,
                (data) => {
                    this.profile = data;
                    this.newPhone = this.profile.phoneNumber;
                });
        }

        isPhoneVerified() {
            return (this.newPhone === this.profile.phoneNumber) && this.profile.phoneNumberConfirmed;
        }

        sendPhoneCode() {
            this.sending = true;
            app.ngHttpPost(this.$http,
                app.accountsApiUrl('phone_number'),
                {
                    phoneNumber: this.newPhone
                },
                () => {
                    this.codeSent = true;
                    this.phoneToVerify = this.profile.phoneNumber;
                },
                () => { this.sending = false; }
                );
        }

        submitPhoneCode() {
            this.sending = true;
            app.ngHttpPost(this.$http,
                app.accountsApiUrl('phone_verification'),
                {
                    phoneNumber: this.newPhone,
                    phoneCode: this.phoneCode
                },
                () => {
                    this.profile.phoneNumberConfirmed = true;
                    this.profile.phoneNumber = this.newPhone;
                },
                () => {
                    this.sending = false;
                    this.codeSent = false;
                    this.phoneCode = null;
                }
                );
        }


    } // end of class
} // end of module
