module App.Teachers_Index {

    export interface IScheduleRouteParams extends ng.route.IRouteParamsService {
        userId: number;
        displayName: string;
        sessionRate: number;
    }

    export class TeacherSchedule {

        eventSources: any[];
        calConfig: any;
        displayName: string;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$routeParams, App.Utils.ngNames.$modal, App.Utils.ngNames.$location];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $routeParams: IScheduleRouteParams,
            private $modal: ng.ui.bootstrap.IModalService,
            private $location: ng.ILocationService
            ) {

            $scope.vm = this;

            this.displayName = this.$routeParams.displayName;

            this.eventSources = [
                {
                    events: (start: Moment, end: Moment, timezone: string, callback: (data: any) => void) => {
                        /* FullCalendar passes Start and End as midnights without a timezone. 
                           In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the exact midnight local time.
                           We send the client-side time and the local TimezoneOffset to infer the client's actual time zone. */
                        var timeInfo = App.Utils.getLocalTimeInfo();

                        App.Utils.ngHttpGetNoCache(this.$http,
                            App.Utils.sessionsApiUrl('UserSchedule/' + this.$routeParams.userId),
                            {
                                start: start.toDate().toISOString(),
                                end: end.toDate().toISOString(),
                                localTime: timeInfo.time,
                                localTimezoneOffset: timeInfo.timezoneOffset,
                            },
                            (data) => {
                                angular.forEach(data, (event) => {
                                    App.Model.ScheduleEvent.dataTransform(event, true);

                                    var offer = (event.type === App.Model.ScheduleEvent.Types.Offered) && moment(event.end).isAfter(); // At this point, event has just come from the server and event.end is a string.
                                    var own = angular.isNumber(event.id) && event.id != 0;
                                    event.url = (offer || own) ? 'javascript:;' : null;
                                });
                                callback(data);
                            }
                            );
                    }
                }
            ];

            this.calConfig = {
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'agendaWeek agendaDay'
                },
                defaultView: 'agendaWeek',
                aspectRatio: 0.5,
                allDaySlot: false,
                slotDuration: '00:15',
                timezone: 'local',
                eventClick: (event: App.Model.ICalendarEvent) => {
                    if (event.type === App.Model.ScheduleEvent.Types.Offered && event.end.isAfter()) {
                        var user =
                            {
                                id: this.$routeParams.userId,
                                displayName: this.$routeParams.displayName,
                                sessionRate: this.$routeParams.sessionRate,
                            };
                        App.Utils.openSessionRequestModal(
                            this.$modal,
                            <App.Model.IUser>user,
                            event.start.toDate(),
                            () => { this.callCalendar('refetchEvents'); });
                    }
                    if (event.id != 0) {
                        var dest = '/sessions/learner#/session/' + event.id;
                        window.location.href = dest;
                        //this.$location.url();
                        //this.$scope.$apply(); // It seem is needed inside a directive???;
                    }
                }
            };

        } // end of ctor

        private callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }
    } // end of class TeacherSchedule

}
