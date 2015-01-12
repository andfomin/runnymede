module app.sessions_index {

    export class OwnSchedule extends app.CtrlBase {

        eventSources: any[];
        calConfig: any;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$state];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $state: ng.ui.IStateService
            ) {
            super($scope);

            this.eventSources = [
                {
                    events: (start: Moment, end: Moment, timezone: string, callback: (data: any) => void) => {
                        if (this.authenticated) {
                            /* FullCalendar passes Start and End as midnights without a timezone. 
                               In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the moment in time.
                               We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                            var timeInfo = app.getLocalTimeInfo();

                            app.ngHttpGet(this.$http,
                                app.sessionsApiUrl('own_schedule'),
                                {
                                    localTime: timeInfo.time,
                                    localTimezoneOffset: timeInfo.timezoneOffset,
                                    start: start.toDate().toISOString(),
                                    end: end.toDate().toISOString(),
                                },
                                (data) => {
                                    angular.forEach(data, (event: app.IScheduleEvent) => {
                                        this.styleEvent(event);
                                    });
                                    callback(data);
                                }
                                );
                        }
                    }
                }
            ];

            var dayClick = (date: Moment) => {
                // Allow for a last-moment real-time arrangement.                
                var d = date.toDate();
                if (Date.now() - d.getTime() < 16 * 60 * 1000) {
                    this.modalVacantTime(d, VacantTimeModal.Create);
                }
            };

            var eventClick = (event: app.IScheduleEvent, jsEvent, view) => {
                if ((event.type === app.sessions_utils.EventTypes.VacantTime) && event.end.isAfter()) {
                    this.modalVacantTime(event.start.toDate(), VacantTimeModal.Delete);
                }
                // Open the session page
                if (app.sessions_utils.isSessionEvent(event)) {
                    $state.go('session', { eventId: event.id });
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
                dayClick: (date: Moment) => { dayClick(date); },
                eventClick: (event: app.IScheduleEvent, jsEvent, view) => { eventClick(event, jsEvent, view); },
            };

        } // ctor

        private styleEvent = (event: app.IScheduleEvent) => {
            var cls: string = null;

            if (app.sessions_utils.isVacantTime(event)) {
                cls = app.sessions_utils.EventClasses.VacantTime;
            }
            else
                if (event.type === app.sessions_utils.EventTypes.Request) {
                    cls = app.sessions_utils.EventClasses.Request;
                }
                else
                    if (event.type === app.sessions_utils.EventTypes.Confirmed) {
                        cls = app.sessions_utils.EventClasses.Confirmed;
                    }
                    else
                        if (event.type === app.sessions_utils.EventTypes.CanceledSelf) {
                            cls = app.sessions_utils.EventClasses.CanceledSelf;
                        }
                        else
                            if (event.type === app.sessions_utils.EventTypes.CanceledOther) {
                                cls = app.sessions_utils.EventClasses.CanceledOther;
                            }

            event.className = app.sessions_utils.classNames(cls);
        };

        callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

        createVacantTime = () => {
            this.modalVacantTime(null, VacantTimeModal.Create);
        }

        deleteVacantTime = () => {
            this.modalVacantTime(null, VacantTimeModal.Delete);
        }

        // modalVacantTime can handle both cases, creation and deletion of vacant time.
        modalVacantTime = (start: Date, kind: string) => {
            app.Modal.openModal(this.$modal,
                'app/sessions/vacantTimeModal.html',
                VacantTimeModal,
                {
                    start: start,
                    kind: kind,
                }
                )
                .result
                .finally(() => { this.callCalendar('refetchEvents'); });
        };

    } // class OwnSchedule

    export class VacantTimeModal extends app.Modal {

        start: Date = null;
        end: Date = null;
        startIsOpen: boolean = false;
        endIsOpen: boolean = false;
        now: Date = new Date();

        static Create = 'create';
        static Delete = 'delete';
        kind: string;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.kind = (modalParams.kind || VacantTimeModal.Create);
            this.start = this.minStart();
        } // ctor

        duration = () => {
            if (this.end) {
                var d = moment.duration(this.end.getTime() - this.start.getTime());
                var h = Math.floor(d.asHours());
                var m = Math.floor(d.minutes());
                return (h > 0 ? ' ' + h + ' hour' + (h > 1 ? 's' : '') : '') + (m > 0 ? ' ' + m + ' minutes' : '');
            }
            else
                return null;
        }

        minStart = () => {
            return this.kind === VacantTimeModal.Create
                ? app.sessions_utils.minStart(this.modalParams.start)
                : this.modalParams.start;
        }

        onSetStart = (newDate: Date, oldDate: Date) => {
            this.startIsOpen = false;
        };

        onSetEnd = (newDate: Date, oldDate: Date) => {
            this.endIsOpen = false;
        };

        canOk = () => {
            return !this.busy && this.authenticated
                && (this.start != null)
                && (this.end != null)
                && (this.start < this.end)
                && (this.start >= this.minStart());
        };

        internalOk = () => {
            /* We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
            var timeInfo = app.getLocalTimeInfo();
            var params = {
                start: this.start,
                end: this.end,
                localTime: timeInfo.time,
                localTimezoneOffset: timeInfo.timezoneOffset,
            };
            var url = app.sessionsApiUrl('vacant_time');

            switch (this.kind) {
                case VacantTimeModal.Create:
                    return app.ngHttpPut(
                        this.$http,
                        url,
                        params,
                        () => { toastr.success('Thank you for offering yor time for conversation sessions!'); }
                        );
                case VacantTimeModal.Delete:
                    return this.$http.delete(
                        url,
                        { params: params }
                        )
                        .success(() => { toastr.success('Session time offer deleted.'); })
                        .error(app.logError);
                default:
                    return null;
            };
        };

    }; // class ModalCreateVacantTime

} // end of module app.sessions_index
