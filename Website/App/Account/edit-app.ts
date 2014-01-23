var app = angular.module('app', ['ngRoute']);

app.controller('ProfileCtrl', App.Account_Edit.ProfileCtrl);
app.controller('PasswordCtrl', App.Account_Edit.PasswordCtrl);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Account/' + template; };
    $routeProvider
        .when('/profile', { templateUrl: templateUrl('edit-profile.html'), controller: 'ProfileCtrl', title: 'Edit profile' })
        .when('/password', { templateUrl: templateUrl('edit-password.html'), controller: 'PasswordCtrl', title: 'Change password' })
        .otherwise({ redirectTo: '/profile' });
}]);

// Dynamically change the page title. ?Refactor as a service? See +http://stackoverflow.com/a/17751833/2808337
interface IRootScopeServiceViewTitle extends ng.IRootScopeService {
    title: string;
}

app.run(['$rootScope', function ($rootScope: IRootScopeServiceViewTitle) {
    $rootScope.$on("$routeChangeSuccess", (event, current, previous) => {
        $rootScope.title = current.title;
        window.document.title = current.title + ' \u002d English Cosmos'; // &ndash;
    });
}]);
