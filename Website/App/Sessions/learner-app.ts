var app = angular.module('app', ['AppUtilsNg', 'ngRoute', 'ui.bootstrap', 'ui.calendar', 'ui.bootstrap.datetimepicker', 'chieffancypants.loadingBar']);

app.controller("Offers", App.Sessions_Learner.Offers);
app.controller("Schedule", App.Sessions_Learner.Schedule);
app.controller(App.Sessions_Utils.Session.ClassName, App.Sessions_Learner.Session);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Sessions/' + template; };
    $routeProvider
        .when('/schedule', { templateUrl: templateUrl('learner-schedule.html'), controller: 'Schedule', title: 'Skype Sessions - Schedule' })
        .when('/offers', { templateUrl: templateUrl('learner-offers.html'), controller: 'Offers', title: 'Skype Sessions - Offers' })
        .when('/session/:id', { templateUrl: templateUrl('learner-session.html'), controller: App.Sessions_Learner.Session.ClassName, title: 'Session' })
        .otherwise({ redirectTo: '/schedule' });
}]);

app.config(['datepickerConfig', 'datepickerPopupConfig', function (datepickerConfig: ng.ui.bootstrap.IDatepickerConfig, datepickerPopupConfig: ng.ui.bootstrap.IDatepickerPopupConfig) {
    datepickerConfig.showWeeks = false;
    datepickerPopupConfig.showButtonBar = false;
}]);

App.Utils.useRouteTitle(app);
App.Utils.detectWrongClock(app);

