module app.sessions_utils {

    // Corresponds to dbo.appTypes (SE....)
    export class EventTypes {
        public static VacantTime = 'SES_VT';
        public static Request = 'SESSRQ';
        public static Confirmed = 'SESSCF';
        public static CanceledSelf = 'SESSCS';
        public static CanceledOther = 'SESSCO';

        static SessionTypes = [
            EventTypes.Request,
            EventTypes.Confirmed,
            EventTypes.CanceledSelf,
            EventTypes.CanceledOther
        ];
    }  

    export class EventClasses {
        public static Generic = 'app-event';
        public static VacantTime = 'app-event-vt';
        public static Request = 'app-event-rq';
        public static Confirmed = 'app-event-cf';
        public static CanceledSelf = 'app-event-cs';
        public static CanceledOther = 'app-event-co';
    }  

    // Minimal time slot is 15 minutes. A session may be requested not less than 15 minutes in advance. So the end of the vacant time should be at least 30 minutes ahead.
    export function isVacantTime(event: app.IScheduleEvent) {
        return ((event.type == app.sessions_utils.EventTypes.VacantTime) && moment(event.end).subtract(30, 'minutes').isAfter());
    };

    export function isSessionEvent(event: app.IScheduleEvent) {
        return app.sessions_utils.EventTypes.SessionTypes.indexOf(event.type) >= 0;
    };

    // The time which is at least 15 minutes ahead from now, rounded up to the next nearest quarter hour(i.e. 0, 15, 30, 45 minute)
    // Corresponds to dbo.sesGetEarliestPossibleStartTime()
    export function minStart(start?: Date) {
        var m15 = 15 * 60 * 1000; // Milliseconds in 15 minutes
        var minMsec = Date.now() + m15;
        var minStartMsec = Math.max(((start && start.getTime()) || 0), minMsec);
        var quarterHourMsec = Math.ceil(minStartMsec / m15) * m15;
        return new Date(quarterHourMsec);
    };

    export function classNames(eventClass?: string) {
        var classes = [EventClasses.Generic];
        if (angular.isString(eventClass)) {
            classes.push(eventClass);
        };
        return classes;
    }

    export class ModalCreateSessionRequest extends app.Modal {

        user: app.IUser;
        now: Date = new Date();
        minStart: Date;
        start: Date;
        duration: number;
        message: string = null;
        startIsOpen: boolean = false;

        durations: any[] = [
            { text: '15 minutes', span: 15 },
            { text: '30 minutes', span: 30 },
            { text: '45 minutes', span: 45 },
            { text: '1 hour', span: 60 },
        ];

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.dismissOnError = true;
            this.user = modalParams.user;
            this.minStart = app.sessions_utils.minStart();
            this.start = app.sessions_utils.minStart(modalParams.start && modalParams.start.toDate());
        } // ctor

        getPrice = () => {
            // Corresponds to price calculation in dbo.sesCreateSessionRequest
            var price = this.user.sessionRate * this.duration / 60;
            var rnd = Math.round(price * 100) / 100;
            var res = Number(rnd.toFixed(2));
            //return { price: price, rnd: rnd, res: res };
            return res;
        }

        onSetStart = (newDate: Date, oldDate: Date) => {
            this.startIsOpen = false;
        };

        canOk = () => {
            return !this.busy
                && (this.start != null)
                && (this.duration != null)
                && (this.start >= this.minStart);
        };

        internalOk = () => {
            var timeInfo = app.getLocalTimeInfo();

            return app.ngHttpPost(this.$http,
                app.sessionsApiUrl(),
                {
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timezoneOffset,
                    userId: this.user.id,
                    start: this.start,
                    duration: this.duration,
                    price: this.getPrice(),
                    message: this.message,
                },
                () => { toastr.success('Session request has been sent.'); }
                );
        };

        /* Shortcut to open createSessionRequestModal.html */
        public static Open($modal: ng.ui.bootstrap.IModalService, user: app.IUser, start: moment.Moment, successCallback: () => void) {
            return app.Modal.openModal($modal,
                '/app/sessions/createSessionRequestModal.html',
                ModalCreateSessionRequest,
                {
                    user: user,
                    start: start,
                },
                successCallback,
                'static'
                );
        }

    }; // end of class ModalCreateSessionRequest

} // end of module
