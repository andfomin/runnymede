module app.library {

    export class Friend extends app.CtrlBase {

        friends: app.IUser[] = null;
        friend: any = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$rootScope, app.ngNames.$window];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $rootScope: angular.IRootScopeService,
            private $window: angular.IWindowService
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            if (this.authenticated) {
                this.loadFriends();
            }
            /* ----- End of constructor  ----- */
        }

        loadFriends = () => {
            app.ngHttpGet(this.$http,
                app.friendsApiUrl('active'),
                null,
                (data) => {
                    this.friends = data;
                }
                );
        };

        // The friend value may be a string entered manually or an object from the dropdown list. 
        private getFriendId = () => {
            if (angular.isString(this.friend)) {
                var arr = this.friends.filter((i) => {
                    return i.displayName === this.friend;
                });
                return (arr.length === 1) ? arr[0].id : null;
            } else if (angular.isObject(this.friend)) {
                return this.friend.id;
            }
            else {
                return null;
            }
        };

        canSyncFriend = () => {
            return !this.busy && this.getFriendId();
        };

        load = () => {
            this.$rootScope.$broadcast(ResourceList.Clear);
            if (!this.busy) {
                this.busy = true;
                app.ngHttpGet(this.$http,
                    app.libraryApiUrl('friend_history/' + this.getFriendId()),
                    null,
                    (data) => {
                        if (data) {
                            if (angular.isArray(data.items)) {
                                this.$rootScope.$broadcast(ResourceList.Display, { resources: data.items });
                            }
                            if (data.totalCount == 0) {
                                // One hour is hardcoded in LibraryApiController.GetFriendHistory
                                toastr.warning('The friend has not viewed any resource during the last hour.');
                            }
                        }
                    },
                    () => { this.busy = false; }
                    );
            }
        };

    } // end of class Ctrl

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Friend', Friend)
        .controller('ResourceList', ResourceList)
    ;

} // end of module app.library_friend
