var app = angular.module('app', ['ngRoute']);

app.controller('Profile', App.Account_Edit.Profile);
app.controller('Password', App.Account_Edit.Password);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Account/' + template; };
    $routeProvider
        .when('/profile', { templateUrl: templateUrl('edit-profile.html'), controller: 'Profile', title: 'Edit profile' })
        .when('/password', { templateUrl: templateUrl('edit-password.html'), controller: 'Password', title: 'Change password' })
        .otherwise({ redirectTo: '/profile' });
}]);

App.Utils.useRouteTitle(app);

