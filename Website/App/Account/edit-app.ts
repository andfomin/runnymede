module app.account_edit {

    class StateConfig {
        static $inject = [app.ngNames.$stateProvider, app.ngNames.$urlRouterProvider];
        constructor(
            private $stateProvider: ng.ui.IStateProvider,
            private $urlRouterProvider: ng.ui.IUrlRouterProvider
            ) {
            var title = 'Edit profile';
            var templateUrl = (template: string) => { return '/app/account/' + template; };

            var personal: ng.ui.IState = {
                name: 'personal',
                url: '/personal',
                templateUrl: templateUrl('edit-personal.html'),
                controller: app.account_edit.Personal,
                data: {
                    title: title,
                    secondaryTitle: 'Personal information',
                },
            };
            var logins: ng.ui.IState = {
                name: 'logins',
                url: '/logins?error',
                templateUrl: templateUrl('edit-logins.html'),
                controller: app.account_edit.Logins,
                reloadOnSearch: false,
                data: {
                    title: title,
                    secondaryTitle: 'Manage logins',
                },
            };
            var teacher: ng.ui.IState = {
                name: 'teacher',
                url: '/teacher',
                templateUrl: templateUrl('edit-teacher.html'),
                controller: app.account_edit.Teacher,
                data: {
                    title: title,
                    secondaryTitle: 'Teacher settings',
                },
            };
            $stateProvider
                .state(personal)
                .state(logins)
                .state(teacher)
            ;
            $urlRouterProvider.otherwise(personal.url);
        }
    };

    angular.module(app.myAppName, ['ui.router', 'ngUpload', 'angular-loading-bar'])
        .config(StateConfig)
        .run(app.StateTitleSyncer)
    ;

}