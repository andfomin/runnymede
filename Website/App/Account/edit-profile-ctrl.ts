module App.Account_Edit {

    export class ProfileCtrl {

        profile: App.Model.IUser;

        sending: boolean;
        displayNameChanged: boolean;
        skypeChanged: boolean;
        rateChanged: boolean;
        anmtChanged: boolean;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
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

        saveTutor(form: ng.IFormController) {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var rateDirty = (<any>form).rate.$dirty ? true : false;
                var anmtDirty = (<any>form).announcement.$dirty ? true : false;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('TutorProfile'),
                    {
                        rate: rateDirty ? this.profile.rate : null,
                        announcement: anmtDirty ? this.profile.announcement : null,
                    },
                    () => {
                        form.$setPristine();
                        this.rateChanged = rateDirty;
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
            this.rateChanged = false;
            this.anmtChanged = false;
        }

    } // end of class
} // end of module
