var app = angular.module('app', ['AppUtilsNg', 'ngRoute', 'chieffancypants.loadingBar']);

app.controller("Add", App.Resources_Youtube.Add);
//app.controller("Schedule", App.Sessions_Learner.Schedule);
//app.controller(App.Sessions_Utils.Session.ClassName, App.Sessions_Learner.Session);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Resources/' + template; };
    $routeProvider
        .when('/add', { templateUrl: templateUrl('youtube-add.html'), controller: 'Add', title: 'YouTube resources - Share a video' })
        .otherwise({ redirectTo: '/add' });
}]);

App.Utils.useRouteTitle(app);

