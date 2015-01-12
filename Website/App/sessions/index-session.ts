module app.sessions_index {

    export class Session {

        session: app.ISession = null;
        messages: app.IMessage[] = [];

        selfUserId: number;
        hostUser: app.IUser;
        guestUser: app.IUser;
        otherUser: app.IUser;
        cancellationUser: app.IUser = null;

        modalParams: any = {
            confirm: {
                header: 'Confirm the Skype session',
                action: 'Confirm',
            },
            cancel: {
                header: 'Cancel the Skype session',
                action: 'Cancel',
            },
            dispute: {
                header: 'Dispute the Skype session',
                action: 'Dispute',
            },
            sendMessage: {
                header: 'Send a new message',
                action: 'SendMessage',
            },
        };

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$stateParams,
            app.ngNames.$interval, app.ngNames.$timeout];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $stateParams: ng.ui.IStateParamsService,
            $interval: ng.IIntervalService,
            private $timeout: ng.ITimeoutService
            ) {
            $scope.vm = this;
            var selfUser = app.getSelfUser();
            this.selfUserId = selfUser && selfUser.id;
            this.getSessionDetails();
            var interval = $interval(() => { this.watchChanges(this.getSessionDetails); }, 20000);
            $scope.$on('$destroy', () => { $interval.cancel(interval); });

        } // end of ctor

        getSessionDetails = () => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(),
                {
                    eventId: this.$stateParams['eventId'],
                },
                (data) => {
                    this.session = data.session;
                    this.messages = data.messages;

                    var s = data.session;

                    this.hostUser = <app.IUser>{
                        id: s.hostUserId,
                        displayName: s.hostDisplayName,
                        skypeName: s.hostSkypeName,
                    };

                    this.guestUser = <app.IUser>{
                        id: s.guestUserId,
                        displayName: s.guestDisplayName,
                        skypeName: s.guestSkypeName,
                    };

                    if (this.selfUserId === s.hostUserId) {
                        this.otherUser = this.guestUser;
                    } else
                        if (this.selfUserId === s.guestUserId) {
                            this.otherUser = this.hostUser;
                        };

                    if (s.cancellationUserId == s.hostUserId) {
                        this.cancellationUser = this.hostUser;
                    } else
                        if (s.cancellationUserId == s.guestUserId) {
                            this.cancellationUser = this.guestUser;
                        };
                }
                );
        }

        watchChanges = (callback: () => void) => {
            app.ngHttpGet(this.$http,
                app.sessionsApiUrl(this.session.id + '/message_count'),
                null,
                (data) => {
                    if (this.messages.length !== +data) {
                        callback();
                    }
                }
                );
        }

        duration = () => {
            return this.session && Math.round((Date.parse(this.session.end) - Date.parse(this.session.start)) / 60000);
        }

        loadMessageText = (m: app.IMessage) => {
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
                                    () => { m.receiveTime = 'at this pageview'; }
                                    );
                            },
                                1000);
                        }
                    })
                    .error(app.logError);
            }
        };

        isUnread = (m: app.IMessage) => {
            return (m.recipientUserId == this.selfUserId) && !m.receiveTime;
        };

        canConfirm = () => {
            return this.session
                && (this.session.hostUserId === this.selfUserId)
                && moment(this.session.end).isAfter()
                && !this.session.confirmationTime
                && !this.session.cancellationTime;
        };

        canCancel = () => {
            return this.session
                && moment(this.session.start).isAfter()
                && !this.session.cancellationTime;
        };

        canDispute = () => {
            return this.session
                && moment(this.session.start).isBefore()
                && this.session.confirmationTime
                && !this.session.cancellationTime
                && this.session.price;
        };

        canShowSkype = () => {
            return this.session
                && this.session.confirmationTime
                && !this.session.cancellationTime;
        };

        modalSessionAction = (params) => {
            app.Modal.openModal(this.$modal,
                'sessionActionModal',
                SessionActionModal,
                {
                    sessionId: this.session.id,
                    action: params.action,
                    header: params.header,
                },
                () => { this.getSessionDetails(); }
                );
        };

    } // end of class Session

    export class SessionActionModal extends app.Modal {

        header: string;
        message: string;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.header = modalParams.header;
        } // ctor

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.sessionsApiUrl(this.modalParams.sessionId + '?action=' + this.modalParams.action),
                {
                    message: this.message,
                }
                );
        };
    }; // class Modal

} // end of module app.sessions_index
