module app.sessions_index {

    export class FriendSchedules extends app.CtrlBase {

        eventSources: any[];
        calConfig: any;
        users: IScheduleUser[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService
            ) {
            super($scope);

            this.eventSources = [
                {
                    events: (start: moment.Moment, end: moment.Moment, timezone: string, callback: (data: any) => void) => {
                        if (this.authenticated) {
                            /* FullCalendar passes Start and End as midnights without a timezone. 
                               In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the moment in time.
                               We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                            var timeInfo = app.getLocalTimeInfo();

                            app.ngHttpGet(this.$http,
                                app.sessionsApiUrl('friend_schedules'),
                                {
                                    localTime: timeInfo.time,
                                    localTimezoneOffset: timeInfo.timezoneOffset,
                                    start: start.toDate().toISOString(),
                                    end: end.toDate().toISOString(),
                                },
                                (data) => {
                                    this.users = data;
                                    var events = [];
                                    angular.forEach(this.users, (user: IScheduleUser) => {
                                        angular.forEach(user.scheduleEvents, (event: app.IScheduleEvent) => {
                                            var rate = (user.sessionRate * 1.0).toFixed(2);
                                            event.title = user.displayName + ' (' + rate + ')';
                                            var cls = app.sessions_utils.isVacantTime(event)
                                                ? app.sessions_utils.EventClasses.VacantTime
                                                : null;
                                            event.className = app.sessions_utils.classNames(cls);
                                            events.push(event);
                                        });
                                    });
                                    callback(events);
                                }
                                );
                        }
                    }
                }
            ];

            var eventClick = (event: app.IScheduleEvent, jsEvent, view) => {
                if (app.sessions_utils.isVacantTime(event)) {
                    var user = app.arrFind(this.users, (i) => { return i.id === event.userId; });
                    var modalInstance = app.sessions_utils.ModalCreateSessionRequest.Open($modal, user, (event && event.start), null);
                    modalInstance.result.finally(() => { this.callCalendar('refetchEvents'); });
                }
            };

            this.calConfig = {
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'agendaWeek agendaDay'
                },
                defaultView: 'agendaWeek',
                height: 'auto',
                allDaySlot: false,
                slotDuration: '00:15',
                timezone: 'local',
                eventClick: (event: app.IScheduleEvent, jsEvent, view) => { eventClick(event, jsEvent, view); },
            };

        } // ctor

        callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

    } // class FriendSchedules

} // end of module app.sessions_index
