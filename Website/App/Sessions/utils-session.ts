module App.Sessions_Utils {

    export interface ISessionRouteParams extends ng.route.IRouteParamsService {
        id: number;
    }

    export interface IModalParams {
        eventId: number;
        header: string;
        action: string;
    }

    export class Session {

        modalParams: any = {
            confirm: {
                header: 'Confirm the Skype session',
                action: App.Model.ScheduleEvent.Types.Confirmed,
            },
            cancel: {
                header: 'Cancel the Skype session',
                action: App.Model.ScheduleEvent.Types.CancelledUS + App.Model.ScheduleEvent.Types.CancelledSU,
            },
            dispute: {
                header: 'Dispute the Skype session',
                action: App.Model.ScheduleEvent.Types.Disputed
            },
        };

        eventId: number = null;
        event: App.Model.IScheduleEvent = null;
        messages: any[] = [];
        message: string = null;

        static ClassName: string = 'Session';

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$routeParams, App.Utils.ngNames.$filter,
            App.Utils.ngNames.$route, App.Utils.ngNames.$modal
        ];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            public $http: ng.IHttpService,
            private $routeParams: ISessionRouteParams,
            private $filter: ng.IFilterService,
            private $route: ng.route.IRouteService,
            public $modal: ng.ui.bootstrap.IModalService
            ) {

            $scope.vm = this;
            this.eventId = this.$routeParams.id;
            this.getSessionDetails();
            this.doPooling();
        } // end of ctor

        private doPooling = () => {
            if (this.$route.current.controller == Session.ClassName) {
                this.watchChanges(this.getSessionDetails);
                // Do not use setInterval. Otherwise there will be many timer instances. The controller is re-created on each entry of the route but the timer is not destroyed on route exit.
                window.setTimeout(() => {
                    this.doPooling();
                },
                    20000);
            }
        }

        getSessionDetails = () => {
            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.sessionsApiUrl('SessionDetails/' + this.eventId),
                null,
                (data) => {
                    this.event = new App.Model.ScheduleEvent(data.event);
                    this.messages = data.messages;
                }
                );
        }

        watchChanges = (callback: () => void) => {
            App.Utils.ngHttpGetNoCache(this.$http,
                App.Utils.sessionsApiUrl('SessionMessageCount/' + this.eventId),
                null,
                (data) => {
                    if (this.messages.length !==  +data) {
                        callback();
                    }
                }
                );
        }

        canSendMessage = () => {
            return this.message && (this.message.length > 0);
        }

        sendMessage = () => {
            App.Utils.ngHttpPost(this.$http,
                App.Utils.sessionsApiUrl('Message'),
                {
                    eventId: this.eventId,
                    message: this.message,
                },
                () => {
                    this.message = null;
                    this.getSessionDetails();
                }
                );
        }

        showModal = (params) => {
            var modalInstance = this.$modal.open({
                templateUrl: '/App/Sessions/utils-sessionModal.html',
                controller: Modal,
                resolve: {
                    modalParams: () => {
                        params.eventId = this.eventId;
                        return params;
                    }
                }
            });

            modalInstance.result.then(
                () => { this.getSessionDetails() },
                null
                );
        }

    } // end of class App.Sessions_Utils.Session

    export class Modal {

        header: string;
        sending: boolean = false;
        message: string;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modalInstance, 'modalParams'];

        constructor(
            private $scope: App.Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private modalParams: IModalParams
            ) {

            $scope.vm = this;
            this.header = modalParams.header;
        } // ctor

        ok = () => {
            this.sending = true;

            App.Utils.ngHttpPost(this.$http,
                App.Utils.sessionsApiUrl('Action'),
                {
                    eventId: this.modalParams.eventId,
                    action: this.modalParams.action,
                    message: this.message,
                },
                null,
                this.$modalInstance.close
                );
        };

        cancel = () => {
            this.$modalInstance.dismiss();
        };

    }; // class Modal

} // end of module
