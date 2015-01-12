module app.sessions_index {

    class StateConfig {
        static $inject = [app.ngNames.$stateProvider, app.ngNames.$urlRouterProvider];
        constructor(private $stateProvider: ng.ui.IStateProvider, private $urlRouterProvider: ng.ui.IUrlRouterProvider) {
            var title = 'Sessions';
            var offers: ng.ui.IState = {
                name: 'offers',
                url: '/offers',
                templateUrl: 'app/sessions/index-offers.html',
                controller: app.sessions_index.Offers,
                data: {
                    title: title,
                    secondaryTitle: 'Search offers',
                },
            };
            var friendSchedules: ng.ui.IState = {
                name: 'friendSchedules',
                url: '/friend-schedules',
                templateUrl: 'app/sessions/index-friendSchedules.html',
                controller: app.sessions_index.FriendSchedules,
                data: {
                    title: title,
                    secondaryTitle: 'Schedules of your friends',
                },
            };
            var schedule: ng.ui.IState = {
                name: 'ownSchedule',
                url: '/schedule',
                templateUrl: 'app/sessions/index-ownSchedule.html',
                controller: app.sessions_index.OwnSchedule,
                data: {
                    title: title,
                    secondaryTitle: 'Your schedule',
                },
            };
            var session: ng.ui.IState = {
                name: 'session',
                url: '/session/:eventId',
                templateUrl: 'app/sessions/index-session.html',
                controller: app.sessions_index.Session,
                data: {
                    title: title,
                    secondaryTitle: 'Session details',
                },
            };
            $stateProvider
                .state(offers)
                .state(friendSchedules)
                .state(schedule)
                .state(session)
            ;
            $urlRouterProvider.otherwise('/offers');
        }
    };

    class DatepickerConfig {
        static $inject = [app.ngNames.datepickerConfig, app.ngNames.datepickerPopupConfig];
        constructor(datepickerConfig: ng.ui.bootstrap.IDatepickerConfig, datepickerPopupConfig: ng.ui.bootstrap.IDatepickerPopupConfig) {
            datepickerConfig.showWeeks = false;
            datepickerPopupConfig.showButtonBar = false;
        }
    }

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.router', 'ui.calendar', 'ui.bootstrap.datetimepicker', 'angular-loading-bar'])
        .config(StateConfig)
        .config(DatepickerConfig)
        .config(app.HrefWhitelistConfig)
        .run(app.StateTitleSyncer)
        .run(app.WrongClockDetector)
        .controller('UserItem', app.sessions_index.UserItem)
    ;

} // end of module app.sessions_index