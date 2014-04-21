module App.Account_Edit {

    export class Teacher {

        profile: App.Model.IUser;

        sending: boolean;
        displayNameChanged: boolean;
        skypeChanged: boolean;
        reviewRateChanged: boolean;
        sessionRateChanged: boolean;
        anntChanged: boolean;
        codeSent: boolean = false;
        phoneCode: string;
        newPhone: string;
        phoneToVerify: string;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.load();

        } // end of ctor
        
        private load = () => {
            App.Utils.ngHttpGet(this.$http,
                App.Utils.accountApiUrl('TeacherProfile'),
                (data) => {
                    this.profile = data;
                    this.newPhone = this.profile.phoneNumber;
                });
        }

        clearChanged() {
            this.reviewRateChanged = false;
            this.sessionRateChanged = false;
            this.anntChanged = false;
        }

        saveTeacher(form: ng.IFormController) {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var reviewRateDirty = (<any>form).reviewRate.$dirty ? true : false;
                var sessionRateDirty = (<any>form).sessionRate.$dirty ? true : false;
                var anntDirty = (<any>form).announcement.$dirty ? true : false;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('TeacherProfile'),
                    {
                        reviewRate: reviewRateDirty ? this.profile.reviewRate : null,
                        sessionRate: sessionRateDirty ? this.profile.sessionRate : null,
                        announcement: anntDirty ? this.profile.announcement : null,
                    },
                    () => {
                        form.$setPristine();
                        this.reviewRateChanged = reviewRateDirty;
                        this.sessionRateChanged = sessionRateDirty;
                        this.anntChanged = anntDirty;
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
            App.Utils.ngHttpPost(this.$http,
                Utils.accountApiUrl('PhoneNumber'),
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
            App.Utils.ngHttpPost(this.$http,
                Utils.accountApiUrl('PhoneVerification'),
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
