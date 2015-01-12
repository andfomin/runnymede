module app.account_edit {

    export class Teacher {

        profile: app.IUser;

        recordingRateChanged: boolean;
        writingRateChanged: boolean;
        sessionRateChanged: boolean;
        sending: boolean;
        codeSent: boolean = false;
        phoneCode: string;
        newPhone: string;
        phoneToVerify: string;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService
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

        clearChanged() {
            this.recordingRateChanged = false;
            this.writingRateChanged = false;
            this.sessionRateChanged = false;
        }

        saveTeacher(form: ng.IFormController) {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var recordingRateDirty = !!(<any>form).recordingRate.$dirty;
                var writingRateDirty = !!(<any>form).writingsRate.$dirty;
                var sessionRateDirty = !!(<any>form).sessionRate.$dirty;

                app.ngHttpPut(this.$http,
                    app.accountsApiUrl('teacher_profile'),
                    {
                        recordingRate: recordingRateDirty ? this.profile.recordingRate : null,
                        writingRate: writingRateDirty ? this.profile.writingRate : null,
                        sessionRate: sessionRateDirty ? this.profile.sessionRate : null,
                    },
                    () => {
                        form.$setPristine();
                        this.recordingRateChanged = recordingRateDirty;
                        this.writingRateChanged = writingRateDirty;
                        this.sessionRateChanged = sessionRateDirty;
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
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
