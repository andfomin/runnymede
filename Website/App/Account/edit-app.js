var app = angular.module('app', ['ngRoute']);

app.controller('ProfileCtrl', App.Account_Edit.ProfileCtrl);
app.controller('PasswordCtrl', App.Account_Edit.PasswordCtrl);

app.config([
    '$routeProvider',
    function ($routeProvider) {
        var templateUrl = function (template) {
            return '/App/Account/' + template;
        };
        $routeProvider.when('/profile', { templateUrl: templateUrl('edit-profile.html'), controller: 'ProfileCtrl', title: 'Edit profile' }).when('/password', { templateUrl: templateUrl('edit-password.html'), controller: 'PasswordCtrl', title: 'Change password' }).otherwise({ redirectTo: '/profile' });
    }
]);

app.run([
    '$rootScope',
    function ($rootScope) {
        $rootScope.$on("$routeChangeSuccess", function (event, current, previous) {
            $rootScope.title = current.title;
            window.document.title = current.title + ' \u002d English Cosmos';
        });
    }
]);
//# sourceMappingURL=edit-app.js.map
