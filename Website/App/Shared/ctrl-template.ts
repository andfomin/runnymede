module App._ {

    export class Ctrl {

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            )
        {
            $scope.vm = this;
        } // end of ctor

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App._.Ctrl);
