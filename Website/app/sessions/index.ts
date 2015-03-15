module app.sessions {

    export class Index extends app.CtrlBase {

        calConfig: FullCalendar.Options;
        eventSources: any[];

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $modal: angular.ui.bootstrap.IModalService
            ) {
            super($scope);

            this.calConfig = {
                allDaySlot: false,
                defaultView: 'agendaThreeDay',
                eventClick: (event, jsEvent, view) => { this.eventClick(<ISession>event, jsEvent, view); },
                header: {
                    left: 'agendaThreeDay,agendaWeek prev,next today',
                    center: 'title',
                    right: ''
                },
                height: <any>'auto',
                slotDuration: '00:30',
                timezone: 'local',
                views: {
                    agendaThreeDay: {
                        type: 'agenda',
                        duration: { days: 3 },
                        buttonText: '3 day'
                    }
                }
            };

            this.eventSources = [
                {
                    events: (start: moment.Moment, end: moment.Moment, timezone: string, callback: (data: any) => void) => {
                        /* FullCalendar passes Start and End as midnights without a timezone. 
                           In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the moment in time.
                           We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                        var timeInfo = app.getLocalTimeInfo();

                        app.ngHttpGet(this.$http,
                            app.sessionsApiUrl(''),
                            {
                                localTime: timeInfo.time,
                                localTimezoneOffset: timeInfo.timezoneOffset,
                                start: start.toDate().toISOString(),
                                end: end.toDate().toISOString(),
                            },
                            (data) => {
                                if (angular.isArray(data)) {
                                    data.forEach((i) => { this.styleEvent(i); });
                                    callback(data);
                                }
                            }
                            );
                    }
                }
            ];

        } // ctor

        callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

        private styleEvent = (session: ISession) => {
            var classes = [CssClasses.Generic];

            if (session.learnerUserId) {
                classes.push(CssClasses.Booked);
            }
            else
                if (!session.learnerUserId) {
                    classes.push(CssClasses.Proposed);
                    session.title = 'Price: ' + session.price;
                }

            session.className = classes;
        };

        private eventClick = (session: ISession, jsEvent, view) => {
            if (session.learnerUserId) {
                app.Modal.openModal(this.$modal,
                    '/app/sessions/showSessionDetails.html',
                    ShowSessionDetailsModal,
                    {
                        session: session,
                    }
                    );
            }
            else {
                if (this.authenticated) {
                    app.Modal.openModal(this.$modal,
                        '/app/sessions/bookSessionModal.html',
                        BookSessionModal,
                        {
                            session: session,
                        },
                        null,
                        'static'
                        )
                        .result
                        .finally(() => { this.callCalendar('refetchEvents'); }
                        );
                }
                else {
                    toastr.warning(app.notAuthenticatedMessage);
                }
            }
        }

    } // end of class Index

    export class BookSessionModal extends app.Modal {

        session: ISession;
        duration: number;

        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.session = modalParams.session;
            this.duration = moment(this.session.end).diff(moment(this.session.start), 'minutes');
        } // ctor

        internalOk = () => {
            var timeInfo = app.getLocalTimeInfo();

            return app.ngHttpPost(this.$http,
                app.sessionsApiUrl('booking'),
                {
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                    start: this.session.start,
                    end: this.session.end,
                    price: this.session.price,
                    teacherUserId: this.session.teacherUserId,
                },
                () => { toastr.success('Thank you for booking a session.'); }
                );
        };

    } // end of class BookSessionModal

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.calendar', 'angular-loading-bar'])
        .config(app.HrefWhitelistConfig)
        .run(app.WrongClockDetector)
        .controller('Index', app.sessions.Index);

} // end of module app.sessions
  