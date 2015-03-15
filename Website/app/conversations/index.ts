module app.conversations {

    export class Index extends app.CtrlBase {

        offered: boolean;
        users: app.IUser[];

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            /* ----- Constructor  ----- */
            super($scope);
            this.load();

            /* ----- End of constructor  ----- */
        }

        load = () => {
            app.ngHttpGet(this.$http,
                app.converstionsApiUrl(),
                null,
                (data) => {
                    if (data) {
                        this.offered = data.offered;
                        if (this.offered) {
                            this.users = data.users;
                        }
                    }
                }
                );
        };

        loadPresentation = (user: IUser) => {
            if (!angular.isString(user.presentation)) {
                app.getUserPresentation(this.$http, user.id,(data) => { user.presentation = data; });
            }
        };

        offer = () => {
            this.invite(app.getSelfUser());
        }

        invite = (toUser: IUser) => {
            this.busy = true;
            return app.ngHttpPost(this.$http,
                app.converstionsApiUrl('' + toUser.id),
                null,
                null,
                () => {
                    this.busy = false;
                    this.load();
                }
                );
        };

    } // end of class

    angular.module(app.myAppName, [app.utilsNg])
        .config(app.HrefWhitelistConfig)
        .controller('Index', app.conversations.Index);

} // end of module conversations

  