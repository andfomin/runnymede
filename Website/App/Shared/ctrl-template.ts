module App._ {

    export class Ctrl {

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            )
        {
            /* ----- Constructor  ----- */
            $scope.vm = this
            /* ----- End of constructor  ----- */
        } 

        f = () => {

        }

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App._.Ctrl);
