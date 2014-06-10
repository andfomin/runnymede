module App.Sessions_Learner {

    export class Offers {

        datepickerOpened: boolean = false;
        minDate: Date = new Date();
        maxDate: Date = new Date(new Date().getTime() + 6 * 24 * 60 * 60 * 1000); // today plus one week

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

        users: App.Model.IUser[] = null;
        selectedUser: App.Model.IUser = null;

        eventSources: any[];
        calConfig: any;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modal, App.Utils.ngNames.$location];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $location: ng.ILocationService
            ) {

            $scope.vm = this;

            this.date = new Date();
            this.date.setHours(0, 0, 0, 0);

            this.eventSources = [
                {
                    events: (start: Moment, end: Moment, timezone: string, callback: (data: any) => any) => {
                        if (this.selectedUser) {
                            App.Utils.ngHttpGetNoCache(this.$http,
                                App.Utils.sessionsApiUrl('UserOffers/' + this.selectedUser.id),
                                {
                                    date: this.date.toISOString(), // Otherwise Angular.$http.get wraps the date string in double-quotes.
                                    startHour: this.startHour,
                                    endHour: this.endHour,
                                },
                                (data) => {
                                    var q = angular.forEach(data, (event) => {
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
                }
            ];

            this.calConfig = this.getCalConfig();

        } // ctor

        private callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

        private getCalConfig = () => {
            return {
                eventSources: this.eventSources,
                header: {
                    left: '', // 'title',
                    center: '',
                    right: '', // 'prev,next'
                },
                defaultView: 'agendaDay',
                height: 9999,
                allDaySlot: false,
                slotDuration: ((this.startHour != null) && this.endHour) ? '00:15' : '01:00',
                minTime: (this.startHour ? this.startHour.toString() : '0') + ':00:00',
                maxTime: (this.endHour ? this.endHour.toString() : '24') + ':00:00',
                timezone: 'local',
                eventClick: (event: App.Model.ICalendarEvent) => {
                    if (this.selectedUser
                        && event.type === App.Model.ScheduleEvent.Types.Offered
                        && event.end.isAfter()) {
                            App.Utils.openSessionRequestModal(
                                this.$modal,
                                this.selectedUser,
                                event.start.toDate(),
                                () => { this.callCalendar('refetchEvents'); });
                    }
                    if (event.id != 0) {
                        this.$location.path('session/' + event.id);
                        this.$scope.$apply(); // It seem is needed inside a directive???;
                    }
                }
            };
        }

        openDatepicker = ($event) => {
            $event.preventDefault();
            $event.stopPropagation();
            this.datepickerOpened = true;
        };

        onFilterStartChange = () => {
            this.hoursEnd = this.hoursStart.slice(this.startHour + 1);
            this.resetCal();
        };

        resetCal = () => {
            this.selectedUser = null;
            this.users = null;

            // Change the calendar setings
            this.callCalendar('destroy');
            window.setTimeout(() => {
                this.callCalendar(this.getCalConfig());
                this.callCalendar('gotoDate', this.date);
            }, 0);
        }

        canSearch = () => {
            return (this.startHour != null)
                && (this.endHour != null)
                && (this.startHour < this.endHour);
        };

        search = () => {
            this.resetCal();

            var timeInfo = App.Utils.getLocalTimeInfo();

            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.sessionsApiUrl('UsersWithOffers'),
                {
                    date: this.date.toISOString(), // Otherwise Angular.$http.get wraps the date string in double-quotes.
                    startHour: this.startHour,
                    endHour: this.endHour,
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                },
                (data) => {
                    this.users = data;
                }
                );
        };

        viewSchedule = (user) => {
            this.selectedUser = user;
            this.callCalendar('refetchEvents');
        };

    } // class Offers

}



