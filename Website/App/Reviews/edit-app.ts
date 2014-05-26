module App.Reviews {

    export class Edit extends App.Reviews.CtrlBase {

        constructor(
            $scope: Utils.IScopeWithViewModel,
            $http: ng.IHttpService
            ) {
            super($scope, $http);
            $scope.vm = this;
        } // end of ctor

    } // end of class
} // end of module

var app = angular.module('app', ['AppUtilsNg', 'chieffancypants.loadingBar']);
app.controller('Ctrl', App.Reviews.Edit);
