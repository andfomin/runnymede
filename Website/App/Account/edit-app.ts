var app = angular.module('app', ['ngRoute', 'ngUpload', 'chieffancypants.loadingBar']);

app.controller('Personal', App.Account_Edit.Personal);
app.controller('Teacher', App.Account_Edit.Teacher);
app.controller('Logins', App.Account_Edit.Logins);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Account/' + template; };
    $routeProvider
        .when('/personal', { templateUrl: templateUrl('edit-personal.html'), controller: 'Personal', title: 'Edit profile' })
        .when('/teacher', { templateUrl: templateUrl('edit-teacher.html'), controller: 'Teacher', title: 'Teacher profile' })
        .when('/logins', { templateUrl: templateUrl('edit-logins.html'), controller: 'Logins', title: 'Manage logins', reloadOnSearch: false })
        .otherwise({ redirectTo: '/personal' });
}]);

App.Utils.useRouteTitle(app);

