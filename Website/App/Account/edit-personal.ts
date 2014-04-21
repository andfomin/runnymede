module App.Account_Edit {

    export class Personal {

        profile: App.Model.IUser;

        sending: boolean;
        displayNameChanged: boolean;
        skypeChanged: boolean;
        isTeacher: boolean;

        email: string;
        emailConfirmed: boolean;
        linkSent: boolean = false;
        newEmail: string;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
            this.isTeacher = (<any>App).isTeacher; // Passed via the page.
            this.load();
        } // end of ctor
        
        private load() {
            App.Utils.ngHttpGet(this.$http,
                App.Utils.accountApiUrl('PersonalProfile'),
                (data) => {
                    this.profile = data;
                    this.refreshAvatarUrls();
                    this.newEmail = this.profile.email;
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

        clearChanged() {
            this.displayNameChanged = false;
            this.skypeChanged = false;
        }

        complete(content) {
            this.refreshAvatarUrls();
        }

        private refreshAvatarUrls() {
            var d = '?_=' + new Date().getTime();

            var src = this.profile.avatarLargeUrl;
            var i = src.indexOf('?_=');
            src = i === -1 ? src : src.substring(0, i);
            this.profile.avatarLargeUrl = src + d;

            src = this.profile.avatarSmallUrl;
            i = src.indexOf('?_=');
            src = i === -1 ? src : src.substring(0, i);
            this.profile.avatarSmallUrl = src + d;
        }

        startUploading() {
            //this.profile.userName = 'Uploading...';
        }

        sendEmailLink() {
            this.sending = true;
            App.Utils.ngHttpPut(this.$http,
                Utils.accountApiUrl('Email'),
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
