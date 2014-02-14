var app = angular.module('app', ['ngRoute']);

app.controller('ListCtrl', App.Relationships_Tutors.ListCtrl);
app.controller('AddCtrl', App.Relationships_Tutors.AddCtrl);

//app.factory('dataService', () => new App.Relationships_Index.DataService());

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Relationships/' + template; };
    $routeProvider
        .when('/list', { templateUrl: templateUrl('tutors-list.html'), controller: 'ListCtrl', title: 'Tutor list' })
        .when('/add', { templateUrl: templateUrl('tutors-add.html'), controller: 'AddCtrl', title: 'Add tutor' })
        .otherwise({ redirectTo: '/list' });
        }]);

