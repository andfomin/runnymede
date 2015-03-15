module app.sessions {

    export class Teacher extends app.CtrlBase {

        calConfig: FullCalendar.Options;
        eventSources: any[];
        selections: ISession[] = [];

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope];

        constructor(
            private $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            private $modal: angular.ui.bootstrap.IModalService,
            private $scope: app.IScopeWithViewModel
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            var interval = $interval(() => { this.callCalendar('refetchEvents'); }, 60000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });

            this.calConfig = {
                allDaySlot: false,
                defaultView: 'agendaWeek',
                eventClick: (event, jsEvent, view) => { this.eventClick(<ISession>event, jsEvent, view); },
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: ''
                },
                height: <any>'auto',
                slotDuration: '00:30',
                timezone: 'local',
            };

            this.eventSources = [
                {
                    events: (start: moment.Moment, end: moment.Moment, timezone: string, callback: (data: any) => void) => {
                        this.selections = [];

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
                            (data: any[]) => {
                                if (angular.isArray(data)) {
                                    data.forEach((i) => { this.styleEvent(i); });
                                    callback(data);
                                }
                            }
                            );
                    }
                }
            ];

            /* ----- End of constructor  ----- */
        }

        callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

        private styleEvent = (session: ISession) => {
            var classes = [CssClasses.Generic];

            if (session.learnerUserId) {
                classes.push(CssClasses.Booked);
            }
            else
                if (session.teacherUserId) {
                    classes.push(CssClasses.Accepted);
                }
                else
                    if (!session.teacherUserId) {
                        classes.push(CssClasses.Proposed);
                        session.title = 'Cost: ' + session.cost;
                    }

            if (this.selections.indexOf(session) != -1) {
                classes.push(CssClasses.Selected);
            }

            session.className = classes;
        };

        private eventClick = (session: ISession, jsEvent, view) => {
            var selectable = !session.teacherUserId;
            if (selectable) {
                var removed = app.arrRemove(this.selections, session);
                if (!removed) {
                    this.selections.push(session);
                }
                this.styleEvent(session);
                this.callCalendar('updateEvent', session);
            }

            if (session.learnerUserId) {
                app.Modal.openModal(this.$modal,
                    '/app/sessions/showSessionDetails.html',
                    ShowSessionDetailsModal,
                    {
                        session: session,
                    }
                    );
            }
        };

        acceptProposals = () => {
            this.busy = true;
            app.ngHttpPost(this.$http,
                app.sessionsApiUrl('accepted_proposals'),
                this.selections.map((i) => {
                    return {
                        id: i.id,
                        cost: i.cost,
                    };
                }),
                null,
                () => {
                    this.busy = false;
                    this.callCalendar('refetchEvents');
                }
                );
        }


    } // end of class Teacher

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.calendar', 'angular-loading-bar'])
        .config(app.HrefWhitelistConfig)
        .run(app.WrongClockDetector)
        .controller('Teacher', app.sessions.Teacher);

} // end of module sessions

   