module app.friends {

    // Corresponds to dbo.appTypes (CN__..)
    export class ContactTypes {
        public static Added = 'CN__AE'; // Added with email
        public static ReviewRequest = 'CN__RR'; // Exercise review requested
        public static ReviewStarted = 'CN__RS'; // Exercise review started
        public static SessionUser = 'CN__SU'; // Skype session requested by the user/guest is confirmed by the friend/host
        public static SessionFriend = 'CN__SF'; // Skype session requested by the friend/guest is confirmed by the user/host
    }

    export interface IFriendship extends app.IUser {
        userIsActive: boolean;
        friendIsActive: boolean;
        userRecordingRate: string; // number;
        friendRecordingRate: number;
        userWritingRate: string; // number;
        friendWritingRate: number;
        userSessionRate: number;
        friendSessionRate: number;
        userLastContactDate: string;
        friendLastContactDate: string;
        userLastContactType: string;
        friendLastContactType: string;
        // not persisted
        knownUserRecordingRate: string;
        knownUserWritingRate: string;
    }

    export class Index {

        friendships: IFriendship[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService
            ) {
            $scope.vm = this;
            this.loadFriends();
        }

        loadFriends = () => {
            app.ngHttpGet(this.$http,
                app.friendsApiUrl(),
                null,
                (data) => {
                    this.friendships = data;
                    this.friendships.forEach((i) => {
                        i.userRecordingRate = app.formatMoney(i.userRecordingRate);
                        i.userWritingRate = app.formatMoney(i.userWritingRate);
                        i.knownUserRecordingRate = i.userRecordingRate;
                        i.knownUserWritingRate = i.userWritingRate;
                    });
                });
        };

        loadPresentation = (f: IFriendship) => {
            if (!angular.isString(f.presentation)) {
                app.getUserPresentation(this.$http, f.id, (data) => { f.presentation = data; });
            }
        };

        getContactIcon = (type: string) => {
            if (type === ContactTypes.Added) {
                return 'fa fa-plus text-success';
            }
            else if (type === ContactTypes.SessionUser || type === ContactTypes.SessionFriend) {
                return 'fa fa-skype';
            }
            else if (type === ContactTypes.ReviewRequest || type === ContactTypes.ReviewStarted) {
                return 'fa fa-pencil-square-o';
            }
            else {
                return null;
            }
        };

        getContactTitle = (type: string) => {
            if (type === ContactTypes.Added) {
                return 'Added manually';
            }
            else if (type === ContactTypes.SessionUser || type === ContactTypes.SessionFriend) {
                return 'Skype session';
            }
            else if (type === ContactTypes.ReviewRequest || type === ContactTypes.ReviewStarted) {
                return 'Exercise review';
            }
            else {
                return null;
            }
        };

        isRecordingRateDirty = (f: IFriendship) => {
            return (f.userRecordingRate != f.knownUserRecordingRate) && (app.isValidAmount(f.userRecordingRate) || f.userRecordingRate === '');
        };

        isWritingRateDirty = (f: IFriendship) => {
            return (f.userWritingRate != f.knownUserWritingRate) && (app.isValidAmount(f.userWritingRate) || f.userWritingRate === '');
        };

        saveRecordingRate = (f: IFriendship) => {
            this.saveRate(f.id, app.ExerciseType.AudioRecording, f.userRecordingRate,
                (data) => {
                    f.userRecordingRate = app.formatMoney(data.rate);
                    f.knownUserRecordingRate = f.userRecordingRate;
                });
        };

        saveWritingRate = (f: IFriendship) => {
            this.saveRate(f.id, app.ExerciseType.WritingPhoto, f.userWritingRate,
                (data) => {
                    f.userWritingRate = app.formatMoney(data.rate);
                    f.knownUserWritingRate = f.userWritingRate;
                });
        };

        saveRate = (friendUserId: number, exerciseType: string, rate: string, callback: ng.IHttpPromiseCallback<any>) => {
            app.ngHttpPut(this.$http,
                app.friendsApiUrl('review_rate'),
                {
                    friendUserId: friendUserId,
                    exerciseType: exerciseType,
                    rate: rate,
                },
                callback
                );
        };

        changeActive = (f: IFriendship) => {
            app.ngHttpPut(this.$http,
                app.friendsApiUrl('active'),
                {
                    friendUserId: f.id,
                    isActive: f.userIsActive,
                }
                );
        };

        showAddFriendModal = () => {
            app.Modal.openModal(this.$modal,
                'addFriendModal',
                AddFriendModal,
                null,
                () => { this.loadFriends(); },
                true
                );
        };

    } // end of class Ctrl

    export class AddFriendModal extends app.Modal {

        email: string;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
        } // ctor

        canAdd = () => {
            return !this.busy && this.authenticated && this.email;
        };

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.friendsApiUrl(),
                {
                    email: this.email,
                },
                () => { toastr.success('Friend has been added.'); });
        };

    }; // end of class addFriendModal

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ngAnimate', 'angular-loading-bar'])
        .controller('Index', Index);

} // end of module

