module app.games {

    export class LuckyYou {

        email: string;
        digits: string;
        showThanks: boolean = false;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;

            /* ----- End of constructor  ----- */
        }

        canOk = () => {
            var emailOk = this.email && app.EMAIL_REGEXP.test(this.email);
            var digitsOk = app.isNumber(this.digits) && (('' + this.digits).length == 2);
            return emailOk && digitsOk && !this.showThanks;
        };

        ok = () => {
            app.ngHttpPost(this.$http,
                app.luckyYouApiUrl('entry'),
                {
                    email: this.email,
                    digits: this.digits,
                },
                () => {
                    this.showThanks = true;
                }
                );
        }

    } // end of class LuckyYou

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('LuckyYou', LuckyYou);

} // end of module app.games

