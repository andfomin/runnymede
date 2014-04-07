module App.Account_Edit {

    export class Profile {

        profile: App.Model.IUser;

        sending: boolean;
        displayNameChanged: boolean;
        skypeChanged: boolean;
        reviewRateChanged: boolean;
        sessionRateChanged: boolean;
        anmtChanged: boolean;

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
                App.Utils.accountApiUrl('Profile'),
                (data) => {
                    this.profile = data;
                });
        }

        save(form: ng.IFormController) {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var displayNameDirty = (<any>form).displayName.$dirty ? true : false;
                var skypeDirty = (<any>form).skype.$dirty ? true : false;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('Profile'),
                    {
                        displayName: displayNameDirty ? this.profile.displayName : null,
                        skype: skypeDirty ? this.profile.skype : null,
                    },
                    () => {
                        form.$setPristine();
                        this.displayNameChanged = displayNameDirty;
                        this.skypeChanged = skypeDirty;
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }

        saveTeacher(form: ng.IFormController) {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var reviewRateDirty = (<any>form).reviewRate.$dirty ? true : false;
                var sessionRateDirty = (<any>form).sessionRate.$dirty ? true : false;
                var anmtDirty = (<any>form).announcement.$dirty ? true : false;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('TeacherProfile'),
                    {
                        reviewRate: reviewRateDirty ? this.profile.reviewRate : null,
                        sessionRate: sessionRateDirty ? this.profile.sessionRate : null,
                        announcement: anmtDirty ? this.profile.announcement : null,
                    },
                    () => {
                        form.$setPristine();
                        this.reviewRateChanged = reviewRateDirty;
                        this.sessionRateChanged = sessionRateDirty;
                        this.anmtChanged = anmtDirty;
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }

        clearChanged() {
            this.displayNameChanged = false;
            this.skypeChanged = false;
            this.reviewRateChanged = false;
            this.sessionRateChanged = false;
            this.anmtChanged = false;
        }

    } // end of class
} // end of module
