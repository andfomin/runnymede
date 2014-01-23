module App.Account_Edit {

    export interface IProfile {
        userName: string;
        displayName: string;
        skype: string;
        isTutor: boolean;
        timezoneName: string;
        rateARec: number;
    }

    export class ProfileCtrl {

        profile: IProfile;

        sending: boolean;
        displayNameChanged: boolean;
        skypeChanged: boolean;
        rateARecChanged: boolean;

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
                this.displayNameChanged = false;
                this.skypeChanged = false;
                var displayNameDirty = (<any>form).displayName.$dirty ? true : false;
                var skypeDirty = (<any>form).skype.$dirty ? true : false;
                var rateARecDirty = (<any>form).rateARec.$dirty ? true : false;

                App.Utils.ngHttpPut(this.$http,
                    Utils.accountApiUrl('Profile'),
                    {
                        displayName: displayNameDirty ? this.profile.displayName : null,
                        skype: skypeDirty ? this.profile.skype : null,
                        rateARec: rateARecDirty ? this.profile.rateARec : null,
                    },
                    () => {
                        form.$setPristine();
                        this.displayNameChanged = displayNameDirty;
                        this.skypeChanged = skypeDirty;
                        this.rateARecChanged = rateARecDirty;
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }

    } // end of class
} // end of module
