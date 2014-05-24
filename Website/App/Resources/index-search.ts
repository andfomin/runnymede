module App.Resources_Index {

    export class Search {

        sending: boolean;

        static $inject = ['$scope', '$http'];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor


    } // end of class
} // end of module

