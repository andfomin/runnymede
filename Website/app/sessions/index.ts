module app.sessions {

    export class Index extends app.sessions.CalendarCtrlBase {

        offers: any[];

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope, CalendarCtrlBase.uiCalendarConfigName];

        constructor(
            $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            uiCalendarConfig: any
            ) {
            super($http, $interval, $modal, $scope, uiCalendarConfig);

            this.calConfig.dayClick = this.dayClick;

        } // ctor

        styleEvent = (session: ISession) => {
            var classes = [CssClasses.Generic];
            if (session.cancellationTime) {
                classes.push(CssClasses.Cancelled);
            } else
                if (session.confirmationTime) {
                    classes.push(CssClasses.Confirmed);
                }
                else
                    if (session.bookingTime) {
                        classes.push(CssClasses.Booked);
                    }
                    else {
                        classes.push(CssClasses.Offered);
                        //session.title = '$' + session.price + '/hour';
                        (<any>session).rendering = 'background';
                        session.price = 2;
                    }
            session.className = classes;
        };

        dayClick = (date: Date, jsEvent, view) => {
            this.offers = null;
            var timeInfo = app.getLocalTimeInfo();
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl('offers'),
                {
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                    start: date.toISOString(),
                    end: moment(date).add(1, 'hours').toISOString(),
                },
                (data: ISession[]) => {
                    if (angular.isArray(data)) {
                        this.offers = data.sort((a, b) => { return (a.start > b.start) ? 1 : ((a.start < b.start) ? -1 : 0); });
                        this.offers.forEach((i) => { (<any>i).duration = moment(i.end).diff(moment(i.start), 'minutes'); });
                    }
                }
                );
        };

        eventClick = (session: ISession, jsEvent, view) => {
            if (session.learnerUserId) {
                app.Modal.openModal(this.$modal,
                    '/app/sessions/showSessionDetails.html',
                    ShowSessionDetailsModal,
                    {
                        session: session,
                    }
                    );
            }
        }

        showBookingDialog = (session: ISession) => {
            if (app.isAuthenticated()) {
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
                    .finally(() => {
                    this.offers = null;
                    this.refetchEvents();
                }
                    );
            }
            else {
                toastr.warning(app.notAuthenticatedMessage);
            }
        };


    } // end of class Index

    export class BookSessionModal extends app.Modal {

        session: ISession;
        duration: number;
        message: string;
        balance: number = null;

        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.session = modalParams.session;
            this.duration = moment(this.session.end).diff(moment(this.session.start), 'minutes');
            this.getConditions();
        } // ctor

        getConditions = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl('conditions'),
                null,
                (data) => {
                    if (data) {
                        this.balance = data.balance || 0;
                    }
                });
        };

        showBalance = () => {
            return angular.isNumber(this.balance) && (this.balance < this.session.price);
        };

        getBuyLink = () => {
            return app.getBuyLink();
        }

        canOk = () => {
            return !this.busy && this.balance && (this.balance > this.session.price);
        }

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
                    message: this.message,
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
  