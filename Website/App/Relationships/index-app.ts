var app = angular.module('app', ['ngRoute']);

app.controller('TutorsCtrl', App.Relationships_Index.TutorsCtrl);
app.controller('AddTutorCtrl', App.Relationships_Index.AddTutorCtrl);

//app.factory('dataService', () => new App.Relationships_Index.DataService());

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Relationships/' + template; };
    $routeProvider
        .when('/tutors', { templateUrl: templateUrl('tutors.html'), controller: 'TutorsCtrl', title: 'Tutor list' })
        .when('/tutors-add', { templateUrl: templateUrl('addTutor.html'), controller: 'AddTutorCtrl', title: 'Add tutor' })
        .otherwise({ redirectTo: '/tutors' });
        }]);

