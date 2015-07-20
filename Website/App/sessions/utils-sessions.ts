module app.sessions {

    export interface ISession extends FullCalendar.EventObject {
        // url?: string; // Chrome weiredly tries to silently send a request to this URL as if it was a real URL. It gets an unrecoverable error for 'javascript:;' and the event fials to render. If the value is malformed, Firefox uncoditionally goes to the URL on click and reports the unknown format to the user.
        teacherUserId: number;
        learnerUserId: number;
        bookingTime: Date;
        confirmationTime: Date;
        cancellationTime: Date;
        cost: number;
        price: number;
        rating: number;
    };

    export class CssClasses {
        public static Generic = 'app-session';
        public static Offered = 'app-session-offered';
        public static Booked = 'app-session-booked';
        public static Confirmed = 'app-session-confirmed';
        public static Cancelled = 'app-session-cancelled';
    }

    export class CalendarCtrlBase {

        calConfig: FullCalendar.Options;
        eventSources: any[];
        eventClick: (session: ISession, jsEvent, view) => void;
        styleEvent: (session: ISession) => void;
        buffer: any;

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope];

        constructor(
            public $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            public $modal: angular.ui.bootstrap.IModalService,
            public $scope: app.IScopeWithViewModel
            ) {
            $scope.vm = this;

            var interval = $interval(() => { this.refetchEvents(); }, 60000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });

            this.calConfig = {
                allDaySlot: false,
                defaultView: 'agendaWeek', // 'myCustomView',
                eventClick: (event, jsEvent, view) => { this.eventClick(<ISession>event, jsEvent, view); },
                firstDay: new Date().getDay(), // the week starts today
                header: {
                    left: '', // 'myCustomView,agendaWeek prev,next today',
                    center: 'title',
                    right: 'prev,next'
                },
                height: <any>'auto',
                slotDuration: '01:00',
                timezone: 'local',
                //views: {
                //    myCustomView: {
                //        type: 'agenda',
                //        duration: { days: 3 },
                //        //buttonText: '3 days',
                //    }
                //}
            };

            this.eventSources = [
                {
                    events: (start: moment.Moment, end: moment.Moment, timezone: string, callback: (data: any) => void) => {
                        // On refetchEvents, fullcalendar first deletes current events, than makes request to the datasource. The rendering on new events occurs after the AJAX request. This causes flicker.
                        // We use buffer simply to prevent flicker during auto-refresh. We prefetch data and clear the buffer each time after use.
                        // TODO  Create an Angular service. IT can cache data for a minute and serve the week as well the day views.
                        var data = this.buffer
                            && start.isSame(this.buffer.start)
                            && end.isSame(this.buffer.end)
                            && this.buffer.data;

                        this.buffer = {
                            start: start,
                            end: end,
                        };

                        if (data) {
                            callback(data);
                        }
                        else {
                            this.loadEvents(start, end, callback);
                        }
                    }
                }
            ];

        } // ctor

        callCalendar = (param1: any, param2?: any) => {
            (<any>this.$scope).calObj.fullCalendar(param1, param2);
        }

        loadEvents = (start: moment.Moment, end: moment.Moment, callback: (data: any) => void) => {
            /* FullCalendar passes Start and End as midnights without a timezone. 
               In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the moment in time.
               We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. 
            */
            var timeInfo = app.getLocalTimeInfo();

            app.ngHttpGet(this.$http,
                app.sessionsApiUrl('schedule'),
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
        };

        refetchEvents = () => {
            if (this.buffer && this.buffer.start && this.buffer.end) {
                this.loadEvents(this.buffer.start, this.buffer.end,
                    (data) => {
                        this.buffer.data = data;
                        this.callCalendar('refetchEvents');
                    });
            }
        };

    } // end of class CalendarCtrlBase

    export class ShowSessionDetailsModal extends app.Modal {

        session: ISession;
        updateSession: () => void;
        otherUser: IUser;
        messages: app.IMessage[] = [];
        message: string;
        selfUser: IUser;
        rating: number = 3;

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modalInstance, app.ngNames.$scope, app.ngNames.$timeout, 'modalParams'];

        constructor(
            $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            private $timeout: angular.ITimeoutService,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.session = modalParams.session;
            this.updateSession = modalParams.updateSession;
            this.selfUser = app.getSelfUser();
            this.getOtherUser();
            this.watchMessages();
            var interval = $interval(() => { this.watchMessages(); }, 10000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });
        } // ctor

        private getOtherUser = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(this.session.id + '/other_user'),
                null,
                (data) => {
                    this.otherUser = data;
                }
                );
        };

        canConfirm = () => {
            return this.selfUser.isTeacher && !this.session.confirmationTime && !this.session.cancellationTime;
        };

        confirmSession = () => {
            app.ngHttpPost(this.$http,
                app.sessionsApiUrl(this.session.id + '/confirmation'),
                null,
                (data) => {
                    this.session.confirmationTime = new Date(data.confirmationTime);
                    this.updateSession();
                }
                );
        };

        cancelSession = () => {
            app.ngHttpPost(this.$http,
                app.sessionsApiUrl(this.session.id + '/cancellation'),
                null,
                (data) => {
                    this.session.cancellationTime = new Date(data.cancellationTime);
                    this.updateSession();
                }
                );
        };

        watchMessages = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(this.session.id + '/message_count'),
                null,
                (data) => {
                    if (this.messages.length !== +data) {
                        this.getMessages();
                    }
                }
                );
        };

        getMessages = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(this.session.id + '/messages'),
                null,
                (data) => {
                    if (angular.isArray(data)) {
                        this.messages = data;
                    }
                }
                );
        };

        loadMessageText = (m: IMessage) => {
            if (!angular.isString(m.text)) {
                // May be cached. 
                this.$http.get(app.sessionsApiUrl('message_text/' + (m.extId || '')))
                    .success((data: any) => {
                    m.text = (data || '');
                    if (this.isUnread(m)) {
                        this.$timeout(() => {
                            app.ngHttpPut(this.$http,
                                app.sessionsApiUrl('message_read/' + m.id + '/' + (m.extId || '')),
                                null,
                                () => { m.receiveTime = 'at this form view'; }
                                );
                        },
                            1000);
                    }
                })
                    .error(app.logError);
            }
        };

        isUnread = (m: IMessage) => {
            return (m.recipientUserId == this.selfUser.id) && !m.receiveTime;
        };

        showMessages = () => {
            return moment(this.session.start).isBefore();
        };

        canSend = () => {
            return !this.busy && !!this.message;
        };

        sendMessage = () => {
            this.busy = true;
            app.ngHttpPost(this.$http,
                app.sessionsApiUrl(this.session.id + '/message'),
                {
                    message: this.message,
                },
                () => {
                    this.message = null;
                    this.getMessages();
                },
                () => { this.busy = false; }
                );
        };

        showSatisfaction = () => {
            return !this.selfUser.isTeacher && moment(this.session.start).isBefore();
        };

        saveRating = () => {
            this.busy = true;
            app.ngHttpPut(this.$http,
                app.sessionsApiUrl(this.session.id + '/rating'),
                {
                    rating: this.session.rating,
                },
                () => {
                    this.message = null;
                    this.getMessages();
                },
                () => { this.busy = false; }
                );
        };

    } // end of class ShowSessionDetailsModal

} // end of module
