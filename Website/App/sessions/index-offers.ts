module app.sessions_index {

    export interface IScheduleUser extends IUser {
        scheduleEvents: IScheduleEvent[];
    }

    export class Offers {

        datepickerOpened: boolean = false;
        minDate: Date = new Date();
        maxDate: Date = new Date(Date.now() + 6 * 24 * 60 * 60 * 1000); // today plus one week

        hoursStart: any[] =
        [
            { sh: 0, t: '12 AM' },
            { sh: 1, t: '\xA01 AM' }, // Non-breakable space is char xA0 (160 dec). We use a monospace font.
            { sh: 2, t: '\xA02 AM' },
            { sh: 3, t: '\xA03 AM' },
            { sh: 4, t: '\xA04 AM' },
            { sh: 5, t: '\xA05 AM' },
            { sh: 6, t: '\xA06 AM' },
            { sh: 7, t: '\xA07 AM' },
            { sh: 8, t: '\xA08 AM' },
            { sh: 9, t: '\xA09 AM' },
            { sh: 10, t: '10 AM' },
            { sh: 11, t: '11 AM' },
            { sh: 12, t: '12 PM' },
            { sh: 13, t: '\xA01 PM' },
            { sh: 14, t: '\xA02 PM' },
            { sh: 15, t: '\xA03 PM' },
            { sh: 16, t: '\xA04 PM' },
            { sh: 17, t: '\xA05 PM' },
            { sh: 18, t: '\xA06 PM' },
            { sh: 19, t: '\xA07 PM' },
            { sh: 20, t: '\xA08 PM' },
            { sh: 21, t: '\xA09 PM' },
            { sh: 22, t: '10 PM' },
            { sh: 23, t: '11 PM' },
            { sh: 24, t: '12 AM' },
        ];

        hoursEnd: any[];

        date: Date;
        startHour: number = null;
        endHour: number = null;

        users: IScheduleUser[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$window, app.ngNames.$modal, app.ngNames.$location];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $window: ng.IWindowService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $location: ng.ILocationService
            ) {
            $scope.vm = this;

            this.date = new Date();
            this.date.setHours(0, 0, 0, 0);
            this.startHour = (new Date()).getHours();
            this.onStartHourChange();
            this.endHour = Math.min(this.startHour + 6, 24);
            this.search();
        } // ctor

        openDatepicker = ($event) => {
            $event.preventDefault();
            $event.stopPropagation();
            this.datepickerOpened = !this.datepickerOpened;
        };

        onStartHourChange = () => {
            this.hoursEnd = this.hoursStart.slice(this.startHour + 1);
        };

        canSearch = () => {
            return (this.startHour != null)
                && (this.endHour != null)
                && (this.startHour < this.endHour);
        };

        search = () => {
            var timeInfo = app.getLocalTimeInfo();

            app.ngHttpGet(this.$http,
                app.sessionsApiUrl('offers'),
                {
                    date: this.date.toISOString(), // Otherwise Angular.$http.get wraps the date string in double-quotes.
                    startHour: this.startHour,
                    endHour: this.endHour,
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                },
                (data) => {
                    this.users = data;

                    var selfUser = app.getSelfUser();
                    var selfUserId = selfUser && selfUser.id;

                    angular.forEach(this.users, (user: IScheduleUser) => {
                        angular.forEach(user.scheduleEvents, (event: app.IScheduleEvent) => {
                            var cls = (app.sessions_utils.isVacantTime(event) && (user.id !== selfUserId))
                                ? app.sessions_utils.EventClasses.VacantTime
                                : null;
                            event.className = app.sessions_utils.classNames(cls);
                            ;
                        });
                    });
                }
                );
        };

        someVacantTime = () => {
            return this.users
                && this.users.some((user) => {
                    return user.scheduleEvents.some((event) => {
                        return app.sessions_utils.isVacantTime(event);
                    });
                });
        };

    } // class Offers

    export class UserItem {
        static $inject = [app.ngNames.$scope, app.ngNames.$modal];
        constructor(
            $scope: any,
            $modal: ng.ui.bootstrap.IModalService
            ) {
            var startHour = $scope.vm.startHour;
            var endHour = $scope.vm.endHour;

            $scope.calOptions = {
                // There is ng-model in the directive. It seem does not provide events, but there are errors in the console if ng-model is missing.
                events: $scope.user.scheduleEvents,
                header: false,
                defaultView: 'agendaDay',
                height: 'auto',
                allDaySlot: false,
                slotDuration: (startHour && endHour) ? '00:15' : '01:00',
                minTime: (startHour ? startHour.toString() : '0') + ':00:00',
                maxTime: (endHour ? endHour.toString() : '24') + ':00:00',
                timezone: 'local',
                eventClick: (event: app.IScheduleEvent) => {
                    if (app.sessions_utils.isVacantTime(event)) {
                        app.sessions_utils.ModalCreateSessionRequest.Open($modal, $scope.user, (event && event.start), null);
                    }
                },
            };

            $scope.showDetails = () => {
                app.Modal.openModal($modal,
                    'userDetailsModal',
                    UserDetailsModal,
                    {
                        user: $scope.user,
                    }
                    );
            };
        }

    } // class UserItem

    export class UserDetailsModal extends app.Modal {

        user: IScheduleUser = null;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.user = modalParams.user;

            if (!angular.isString(this.user.presentation)) {
                app.getUserPresentation(this.$http, this.user.id, (data) => { this.user.presentation = data; });
            }
        } // ctor
    }; // class ModalUserDetails


}