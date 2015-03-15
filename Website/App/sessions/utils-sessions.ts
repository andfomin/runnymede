module app.sessions {

    export interface ISession extends FullCalendar.EventObject {
        // url?: string; // Chrome weiredly tries to silently send a request to this URL as if it was a real URL. It gets an unrecoverable error for 'javascript:;' and the event fials to render. If the value is malformed, Firefox uncoditionally goes to the URL on click and reports the unknown format to the user.
        teacherUserId: number;
        learnerUserId: number;
        cost: number;
        price: number;
        rating: number;
    };

    export class CssClasses {
        public static Generic = 'app-session';
        public static Selected = 'app-session-selected';
        public static Proposed = 'app-session-proposed';
        public static Accepted = 'app-session-accepted';
        public static Booked = 'app-session-booked';
    }

    export class ShowSessionDetailsModal extends app.Modal {

        session: ISession;
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
            this.selfUser = app.getSelfUser();
            this.load();
            this.watchMessages();
            var interval = $interval(() => { this.watchMessages(); }, 10000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });
        } // ctor

        load = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(this.session.id + '/other_user'),
                null,
                (data) => {
                    this.otherUser = data;
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

        saveRating1 = () => {
            toastr.info('Rating: ' + this.session.rating);
        };

    } // end of class ShowSessionDetailsModal

} // end of module
