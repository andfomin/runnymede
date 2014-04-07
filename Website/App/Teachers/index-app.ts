var app = angular.module('app', ['AppUtilsNg', 'ngRoute', 'ui.bootstrap', 'ui.calendar', 'ui.bootstrap.datetimepicker', 'chieffancypants.loadingBar']);

app.controller('Favorites', App.Teachers_Index.Favorites);
app.controller('All', App.Teachers_Index.All);
app.controller("TeacherSchedule", App.Teachers_Index.TeacherSchedule);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Teachers/' + template; };
    $routeProvider
        .when('/favorites', { templateUrl: templateUrl('index-favorites.html'), controller: 'Favorites', title: 'Favorite Teachers' })
        .when('/all', { templateUrl: templateUrl('index-all.html'), controller: 'All', title: 'All Teachers' })
        .when('/teacher-schedule/:userId/:displayName/:sessionRate', { templateUrl: templateUrl('index-teacherSchedule.html'), controller: 'TeacherSchedule', title: 'Teacher\'s Schedule' })
        .otherwise({ redirectTo: '/favorites' });
}]);

app.config(['datepickerConfig', 'datepickerPopupConfig', function (datepickerConfig: ng.ui.bootstrap.IDatepickerConfig, datepickerPopupConfig:ng.ui.bootstrap.IDatepickerPopupConfig) {
    datepickerConfig.showWeeks = false;
    datepickerPopupConfig.showButtonBar = false;
}]);

app.constant('viewSession', new Date().getTime() % 100); // The range can be such small since it is anyway normalized by sin() in SQL.

App.Utils.useRouteTitle(app);

