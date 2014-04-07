module App.Model {

    export interface ICalendarEvent {
        // These properties are supported by FullCalendar
        id: number;
        start: Moment;
        end: Moment;
        title: string;
        textColor: string;
        color: string;
        backgroundColor: string;
        borderColor: string;
        url: string;
        // This is a custom property
        type: string;
    }

    export interface IScheduleEvent {
        id: number;
        start: Moment;
        end: Moment;
        type: string;
        userId: number;
        secondUserId: number;
        price: number;
        creationTime: Moment;
        confirmationTime: Moment;
        cancellationTime: Moment;
        closingTime: Moment;
        userDisplayName: string;
        secondUserDisplayName: string;
        userSkype: string;
        secondUserSkype: string;
    }

    export class ScheduleEvent implements IScheduleEvent {
        id: number;
        start: Moment;
        end: Moment;
        type: string;
        userId: number;
        secondUserId: number;
        userDisplayName: string;
        secondUserDisplayName: string;
        price: number;
        creationTime: Moment;
        confirmationTime: Moment;
        cancellationTime: Moment;
        closingTime: Moment;
        userSkype: string;
        secondUserSkype: string;

        constructor(data: any) {
            this.id = data.id;
            this.start = moment(data.start);
            this.end = moment(data.end);
            this.type = data.type;
            this.userId = data.userId;
            this.secondUserId = data.secondUserId;
            this.price = data.price;
            this.creationTime = moment(data.creationTime);
            this.confirmationTime = data.confirmationTime ? moment(data.confirmationTime) : null;
            this.cancellationTime = data.cancellationTime ? moment(data.cancellationTime) : null;
            this.closingTime = data.closingTime ? moment(data.closingTime) : null;
            this.userDisplayName = data.userDisplayName;
            this.secondUserDisplayName = data.secondUserDisplayName;
            this.userSkype = data.userSkype;
            this.secondUserSkype = data.secondUserSkype;
        } // end of ctor

        public static Types = {
            Offered: 'OFFR', // Offer
            Revoked: 'ROFR', // Revoked Offer
            Requested: 'RQSN', // Requested session
            Confirmed: 'CFSN', // Confirmed session 
            CancelledUS: 'CSUS', // Cancelled Session, by User, i.e. teacher
            CancelledSU: 'CSSU', // Cancelled Session, by SecondUser, i.e. learner
            Closed: 'CLSN', // Closed session 
            Disputed: 'DSSN', // Disputed session 
        };

        public static SessionTypes = [
            ScheduleEvent.Types.Requested,
            ScheduleEvent.Types.Confirmed,
            ScheduleEvent.Types.CancelledUS,
            ScheduleEvent.Types.CancelledSU,
            ScheduleEvent.Types.Closed,
            ScheduleEvent.Types.Disputed,
        ];

        public static dataTransform(event: App.Model.ICalendarEvent, isSecondUser: boolean) {
            event.textColor = 'black';
            //event.url = moment(event.end).isAfter() ? 'javascript:;' : null; // At this point, event has just come from the server and event.end is a string.
            if (event.type === App.Model.ScheduleEvent.Types.Offered) {
                event.title = 'Offer';
                event.backgroundColor = '#5BC0DE'; // btn-info  '#0074AD' appBlue.
                event.borderColor = '#5BC0DE';
            }
            else
                if (event.type === App.Model.ScheduleEvent.Types.Requested) {
                    event.title = 'Requested session';
                    event.backgroundColor = '#FFC82D'; // '#F0AD4E'; // btn-warning
                    event.borderColor = '#FFC82D'; // '#FFC82D' appYellow
                }
                else
                    if (event.type === App.Model.ScheduleEvent.Types.Confirmed) {
                        event.title = 'Confirmed session';
                        event.backgroundColor = '#5CB85C'; // btn-success
                        event.borderColor = '#5CB85C';
                    }
                    else
                        if (event.type === App.Model.ScheduleEvent.Types.CancelledUS) {
                            event.title = 'Cancelled session';
                            event.backgroundColor = isSecondUser ? '#D9534F' : 'gainsboro'; // btn-danger;
                            event.borderColor = '#D9534F'; // btn-danger;
                        }
                        else
                            if (event.type === App.Model.ScheduleEvent.Types.CancelledSU) {
                                event.title = 'Canceled session';
                                event.backgroundColor = isSecondUser ? 'gainsboro' : '#D9534F';// '#901a21' appRed
                                event.borderColor = '#D9534F'; // btn-danger;
                            }
                            else
                                if (event.type === App.Model.ScheduleEvent.Types.Closed) {
                                    event.title = 'Closed session';
                                    event.backgroundColor = 'gainsboro';
                                    event.borderColor = '#5CB85C'; // btn-success
                                }
                                else
                                    if (event.type === App.Model.ScheduleEvent.Types.Disputed) {
                                        event.title = 'Disputed session';
                                        event.backgroundColor = 'gainsboro';
                                        event.borderColor = '#FFC82D'; // btn-warning
                                    }


            return event;
        } // static dataTransform
    } // end of class ScheduleEvent
} // end of module App.Model 

module App.Utils {

    export interface ISessionRequestModalParams {
        user: App.Model.IUser;
        start: Date;
    }

    export function openSessionRequestModal($modal: ng.ui.bootstrap.IModalService, user: App.Model.IUser, start: Date, successCallback: () => void) {
        var modalInstance = $modal.open({
            templateUrl: '/App/Shared/Html/SessionRequestModal.html',
            controller: App.Utils.SessionRequestModal,
            backdrop: 'static',
            resolve: {
                modalParams: () => {
                    var params: App.Utils.ISessionRequestModalParams =
                        {
                            user: user,
                            start: angular.isDefined(start) ? start : null,
                        };
                    return params;
                }
            }
        });

        modalInstance.result
            .then(
            () => { successCallback(); },
            null
            );
    };

    export class SessionRequestModal {

        user: App.Model.IUser;
        start: Date = null;
        duration: number;
        message: string = null;
        sending: boolean = false;
        now: Date = new Date();

        durations: any[] = [
            { text: '15 minutes', span: 15 },
            { text: '30 minutes', span: 30 },
            { text: '45 minutes', span: 45 },
            { text: '1 hour', span: 60 },
        ];

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modalInstance, 'modalParams'];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private modalParams: ISessionRequestModalParams
            ) {

            $scope.vm = this;
            this.user = modalParams.user;

            var coeff = 5 * 60 * 1000; // 5 minutes to milliseconds
            var min = Math.ceil((new Date().valueOf() + coeff) / coeff) * coeff; // The time rounded to 5 minutes which is far than 5 minutes ahead from now.
            this.start = new Date(Math.max((modalParams.start ? modalParams.start.valueOf() : min), min));
        } // ctor

        getPrice = () => {
            return this.user.sessionRate * this.duration / 60;
        }

        canSave = () => {
            return !this.sending
                && (this.start != null)
                && (this.duration != null)
                && (this.start > new Date());
        };

        save = () => {
            this.sending = true;

            var timeInfo = App.Utils.getLocalTimeInfo();

            App.Utils.ngHttpPost(this.$http,
                App.Utils.sessionsApiUrl('Session'),
                {
                    userId: this.user.id,
                    start: this.start,
                    duration: this.duration,
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timeZoneOffset,
                    message: this.message,
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

    }; // end of class SessionRequestModal

} // end of module App.Utils