module app.account_edit {

    export class Personal {

        profile: app.IUser;
        isTeacher: boolean;
        userName: string;
        avatarLargeUrl: string;
        avatarSmallUrl: string;

        sending: boolean = false;
        displayNameChanged: boolean;
        skypeNameChanged: boolean;
        anntChanged: boolean;
        presenChanged: boolean;
        uploadedFile: string;

        //email: string;
        //emailConfirmed: boolean;
        //linkSent: boolean = false;
        //newEmail: string;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$document, app.ngNames.$interval];

        static myScope: angular.IScope;
        //static onFileChange: () => void;

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $document: angular.IDocumentService,
            $interval: angular.IIntervalService
            ) {
            $scope.vm = this;
            var self = app.getSelfUser();
            this.isTeacher = self.isTeacher;
            this.userName = self.userName;
            this.loadProfile();

            // ng-change does not support input["file"]. We attach a handler to the native event on the element.
            (<any>Personal).onFileChange = () => { $scope.$apply(); };
        } // end of ctor

        private loadProfile = () => {
            app.ngHttpGet(this.$http,
                app.accountsApiUrl('personal_profile'),
                null,
                (data) => {
                    this.profile = data;
                    this.refreshAvatarUrls();
                    //this.newEmail = this.profile.email;
                    this.loadPresentation();
                    //(<any>this.$scope).anntForm.$setPristine(); //  IE considers a placeholder as plain content and causes the form to become dirty from the very beginning.
                });
        }

        private loadPresentation = () => {
            app.getUserPresentation(this.$http, this.profile.id, (data) => { this.profile.presentation = data; });
        };

        saveMain = (form: angular.IFormController) => {
            if (form.$valid) {
                this.sending = true;
                this.clearChanged();
                var displayNameDirty = (<any>form).displayName.$dirty ? true : false;
                var skypeDirty = (<any>form).skypeName.$dirty ? true : false;

                app.ngHttpPut(this.$http,
                    app.accountsApiUrl('profile'),
                    {
                        displayName: displayNameDirty ? this.profile.displayName : null,
                        skypeName: skypeDirty ? this.profile.skypeName : null,
                    },
                    () => {
                        form.$setPristine();
                        this.displayNameChanged = displayNameDirty;
                        this.skypeNameChanged = skypeDirty;
                    },
                    () => {
                        this.sending = false;
                    }
                    );
            }
        }

        saveAnnt = (form: angular.IFormController) => {
            if (form.$valid) {
                this.sending = true;
                this.anntChanged = false;
                if ((<any>form).annt.$dirty) {
                    app.ngHttpPut(this.$http,
                        app.accountsApiUrl('announcement'),
                        {
                            announcement: this.profile.announcement,
                        },
                        () => {
                            form.$setPristine();
                            this.anntChanged = true;
                        },
                        () => {
                            this.sending = false;
                        }
                        );
                }
            }
        }

        savePresen = (form: angular.IFormController) => {
            if (form.$valid) {
                this.sending = true;
                this.presenChanged = false;
                if ((<any>form).presen.$dirty) {
                    app.ngHttpPut(this.$http,
                        app.accountsApiUrl('presentation'),
                        {
                            presentation: this.profile.presentation,
                        },
                        () => {
                            form.$setPristine();
                            this.presenChanged = true;
                        },
                        () => {
                            this.sending = false;
                        }
                        );
                }
            }
        }

        clearChanged = () => {
            this.displayNameChanged = false;
            this.skypeNameChanged = false;
        }

        private getFile = () => {
            var input = <any>angular.element(this.$document[0].querySelector('#fileInput'));
            var files = input && input[0].files;
            return (files && (files.length == 1)) ? files[0] : null;
        };

        private getFileKey = (file: any) => {
            return file ? ('' + file.name + file.type + file.size + file.lastModified) : null;
        };

        canUpload = () => {
            var file = this.getFile();
            return file && (this.uploadedFile != this.getFileKey(file)) && (file.size < 10000000);
        };

        uploadComplete = (content) => {
            //var e = angular.element(this.$document[0].querySelector('#fileInput'));
            //var f = e.closest('form').get(0);
            //(<any>f).reset();
            this.uploadedFile = this.getFileKey(this.getFile());

            this.refreshAvatarUrls();
        }

        private refreshAvatarUrls = () => {
            var testUrl = app.getBlobUrl('t', 't'); // Development and production Url formats are different.
            var ac = (testUrl.indexOf('?') == -1 ? '?' : '&') + '_=' + app.anticacher();
            this.avatarLargeUrl = app.getAvatarLargeUrl(this.profile.id) + ac;
            this.avatarSmallUrl = app.getAvatarSmallUrl(this.profile.id) + ac;
            // Top navbar avatar.
            var img = angular.element(this.$document[0].querySelector('#navAvatar'));
            var src = img && (img.length > 0) && img.attr('src');
            if (src && (src.indexOf(this.profile.id.toString()) >= 0)) {
                //var i = src.indexOf('?_=');
                //src = (i === -1 ? src : src.substring(0, i)) + ac;
                img.attr('src', this.avatarSmallUrl);
            }
        };

        startUploading() {
            //console.log('uploading...');
        }

        //sendEmailLink() {
        //    this.sending = true;
        //    app.ngHttpPut(this.$http,
        //        app.accountsApiUrl('email'),
        //        {
        //            email: this.newEmail
        //        },
        //        () => {
        //            this.linkSent = true;
        //        },
        //        () => { this.sending = false; }
        //        );
        //}

    } // end of class
} // end of module
