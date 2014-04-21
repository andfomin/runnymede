module App.Sessions_Teacher {

    export class Schedule {

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

            this.eventSources = [
                {
                    events: (start: Moment, end: Moment, timezone: string, callback: (data: any) => void) => {
                        /* FullCalendar passes Start and End as midnights without a timezone. 
                           In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the moment in time.
                           We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                        var timeInfo = App.Utils.getLocalTimeInfo();

                        App.Utils.ngHttpGetWithParamsNoCache(this.$http,
                            App.Utils.sessionsApiUrl('OwnSchedule'),
                            {
                                start: start.toDate().toISOString(),
                                end: end.toDate().toISOString(),
                                localTime: timeInfo.time,
                                localTimezoneOffset: timeInfo.timezoneOffset,
                            },
                            (data) => {
                                angular.forEach(data, (event) => {
                                    App.Model.ScheduleEvent.dataTransform(event, false);
                                    if ((event.type != App.Model.ScheduleEvent.Types.Offered) || moment(event.end).isAfter()) {
                                        event.url = 'javascript:;';
                                    }
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
                dayClick: (date: Moment) => {
                    var d = date.toDate();
                    // Allow for a last-moment real-time arrangement.
                    if (d.getTime() > new Date().getTime() - 31 * 60 * 1000) {
                        this.showCreateOfferModal(d);
                    }
                },
                eventClick: (event: App.Model.IScheduleEvent, jsEvent, view) => {
                    if ((event.type === App.Model.ScheduleEvent.Types.Offered) && event.end.isAfter()) {
                        this.showRevokeOfferModal(event);
                    }
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

        showCreateOfferModal = (start: Date) => {
            var modalInstance = this.$modal.open({
                templateUrl: 'createOfferModal.html',
                controller: CreateOfferModal,
                resolve: {
                    start: () => {
                        return start;
                    }
                }
            });

            modalInstance.result.then(
                () => { this.callCalendar('refetchEvents'); },
                null
                );
        };

        showRevokeOfferModal = (event) => {
            var modalInstance = this.$modal.open({
                templateUrl: 'revokeOfferModal.html',
                controller: RevokeOfferModal,
                resolve: {
                    event: () => {
                        return event;
                    }
                }
            });

            modalInstance.result.then(
                () => { this.callCalendar('refetchEvents'); },
                null
                );
        };


    } // class Schedule

    export class CreateOfferModal {

        start: Date = null;
        end: Date = null;
        now: Date = new Date();
        sending: boolean = false;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modalInstance, 'start'];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            start: Date
            ) {

            $scope.vm = this;

            var coeff = 15 * 60 * 1000;
            this.start = start
            ? start
            : new Date(Math.ceil(new Date().getTime() / coeff) * coeff); // Round to the next nearest quarter hour.
        } // ctor

        displyDuration = () => {
            var d = moment.duration(this.end.valueOf() - this.start.valueOf());
            var h = Math.floor(d.asHours());
            var m = Math.floor(d.minutes());
            return (h > 0 ? ' ' + h + ' hour' + (h > 1 ? 's' : '') : '') + (m > 0 ? ' ' + m + ' minutes' : '');
        }

        canSave = () => {
            var past = new Date();
            past.setTime(past.getTime() - 17 * 60 * 1000); // milliseconds

            return !this.sending
                && (this.start != null)
                && (this.end != null)
                && (this.start < this.end)
                && (this.start > past);
        };

        save = () => {
            this.sending = true;
            /* We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
            var timeInfo = App.Utils.getLocalTimeInfo();

            App.Utils.ngHttpPut(this.$http,
                App.Utils.sessionsApiUrl('Offer'),
                {
                    start: this.start,
                    end: this.end,
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                },
                () => {
                    this.$modalInstance.close();
                },
                () => { this.sending = false; }
                );
        };

        cancel = () => {
            this.$modalInstance.dismiss('cancel');
        };

    }; // class CreateOfferModal

    export class RevokeOfferModal {

        event: App.Model.ICalendarEvent;
        sending: boolean = false;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modalInstance, 'event'];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            event: App.Model.ICalendarEvent
            ) {

            $scope.vm = this;
            this.event = event;
        } // ctor

        ok = () => {
            this.sending = true;

            this.$http.delete(App.Utils.sessionsApiUrl('Offer/' + this.event.id))
                .success(() => { this.$modalInstance.close(); })
                .error(App.Utils.logError)
                .finally(() => { this.sending = false; });
        };

        cancel = () => {
            this.$modalInstance.dismiss('cancel');
        };

    }; // class RevokeOfferModal

} // module
