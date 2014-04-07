var app = angular.module('app', ['AppUtilsNg', 'ngRoute', 'ui.bootstrap', 'ui.calendar', 'ui.bootstrap.datetimepicker', 'chieffancypants.loadingBar']);

app.controller('Schedule', App.Sessions_Teacher.Schedule);
app.controller(App.Sessions_Utils.Session.ClassName, App.Sessions_Teacher.Session);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Sessions/' + template; };
    //var title = (pageTitle: string) => { return 'Skype Sessions' + (pageTitle ? ' - ' + pageTitle : ''); };
    $routeProvider
        .when('/schedule', { templateUrl: templateUrl('teacher-schedule.html'), controller: 'Schedule', title: 'Skype Sessions - Schedule' })
        .when('/session/:id', { templateUrl: templateUrl('teacher-session.html'), controller: 'Session', title: 'Session' })
        .otherwise({ redirectTo: '/schedule' });
}]);

App.Utils.useRouteTitle(app);
App.Utils.detectWrongClock(app);



