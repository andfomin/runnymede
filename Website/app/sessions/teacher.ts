module app.sessions {

    export class Teacher extends app.sessions.CalendarCtrlBase {

        calConfig: FullCalendar.Options;
        eventSources: any[];
        //selections: ISession[] = [];

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope, CalendarCtrlBase.uiCalendarConfigName];

        constructor(
            $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            uiCalendarConfig: any
            ) {
            super($http, $interval, $modal, $scope, uiCalendarConfig);
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
                    if (session.learnerUserId) {
                        classes.push(CssClasses.Booked);
                        //session.title = 'Cost: ' + session.cost;
                    }
            session.className = classes;
        };

        eventClick = (session: ISession, jsEvent, view) => {
            //var selectable = !session.teacherUserId;
            //if (selectable) {
            //    var removed = app.arrRemove(this.selections, session);
            //    if (!removed) {
            //        this.selections.push(session);
            //    }
            //    this.styleEvent(session);
            //    this.callCalendar('updateEvent', session);
            //}

            if (session.learnerUserId) {
                app.Modal.openModal(this.$modal,
                    '/app/sessions/showSessionDetails.html',
                    ShowSessionDetailsModal,
                    {
                        session: session,
                        updateSession: () => {
                            this.styleEvent(session);
                            this.callCalendar('updateEvent', session);
                        },
                    }
                    );
            }
        };

        //acceptProposals = () => {
        //    this.busy = true;
        //    app.ngHttpPost(this.$http,
        //        app.sessionsApiUrl('accepted_proposals'),
        //        this.selections.map((i) => {
        //            return {
        //                id: i.id,
        //                cost: i.cost,
        //            };
        //        }),
        //        null,
        //        () => {
        //            this.busy = false;
        //            this.refetchEvents();
        //        }
        //        );
        //}

    } // end of class Teacher

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.calendar', 'angular-loading-bar'])
        .config(app.HrefWhitelistConfig)
        .run(app.WrongClockDetector)
        .controller('Teacher', app.sessions.Teacher);

} // end of module sessions

   