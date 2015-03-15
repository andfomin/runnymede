module app._ {

    export class Ctrl {

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;

            /* ----- End of constructor  ----- */
        }

        f = () => {

        }

    } // end of class

    angular.module(app.myAppName, [])
        .controller('Ctrl', app._.Ctrl);

} // end of module

