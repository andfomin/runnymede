module App.Sessions_Learner {

    export class Schedule {

        eventSources: any[];
        calConfig: any;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$location];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $location: ng.ILocationService
            ) {
            $scope.vm = this;

            this.eventSources = [
                {
                    events: (start: Moment, end: Moment, timezone: string, callback: (data: any) => void) => {
                        /* FullCalendar passes Start and End as midnights without a timezone. 
                           In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the exact midnight local time.
                           We send the client-side time and the local TimezoneOffset to infer the client's actual time zone. */
                        var timeInfo = App.Utils.getLocalTimeInfo();

                        App.Utils.ngHttpGetNoCache(this.$http,
                            App.Utils.sessionsApiUrl('OwnSchedule'),
                            {
                                start: start.toDate().toISOString(),
                                end: end.toDate().toISOString(),
                                localTime: timeInfo.time,
                                localTimezoneOffset: timeInfo.timezoneOffset,
                            },
                            (data) => {
                                angular.forEach(data, (event) => {
                                    App.Model.ScheduleEvent.dataTransform(event, true);
                                    event.url = 'javascript:;';
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
                eventClick: (event: App.Model.IScheduleEvent, jsEvent, view) => {
                    if (App.Model.ScheduleEvent.SessionTypes.indexOf(event.type) >= 0) {
                        this.$location.path('session/' + event.id);
                        this.$scope.$apply(); // It seem is needed inside a directive???;
                    }
                }
            };
        } // ctor

        private callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

    } // class Schedule
}