module app.account {

    export class ThankYou {

        email: string;
        //sending: boolean;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$location];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            $location: ng.ILocationService
            ) {
            $scope.vm = this;

            var search = $location.search();
            var email = search && search.email;
            if (email) {
                this.email = decodeURIComponent(email);
            }
            // Clear the email text in the address bar.
            $location.search('email', null); 
        } // end of ctor

    } // end of class

    class LocationConfig {
        static $inject = [app.ngNames.$locationProvider];
        constructor($locationProvider: ng.ILocationProvider) {
            // The typing does not know this kind of parameter.
            var locationProvider = <any>$locationProvider;
            locationProvider.html5Mode({
                enabled: true,
                requireBase: false
            });
        }
    };

    angular.module(app.myAppName, ['angular-loading-bar'])  
        .config(LocationConfig)  
        .controller('ThankYou', ThankYou);

} // end of module

