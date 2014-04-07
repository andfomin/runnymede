module App.Sessions_Learner {

    export class Session extends App.Sessions_Utils.Session {

        constructor(
            $scope: App.Utils.IScopeWithViewModel,
            $http: ng.IHttpService,
            $routeParams: App.Sessions_Utils.ISessionRouteParams,
            $filter: ng.IFilterService,
            $route: ng.route.IRouteService,
            $modal: ng.ui.bootstrap.IModalService
            ) {
            super($scope, $http, $routeParams, $filter, $route, $modal);
        } // end of ctor

        canCancel = () => {
            return this.event
                && ((this.event.type === App.Model.ScheduleEvent.Types.Requested) || (this.event.type === App.Model.ScheduleEvent.Types.Confirmed))
                && (this.event.start.isAfter());
        }

        canDispute = () => {
            var e = this.event;
            return e
                && (e.type === App.Model.ScheduleEvent.Types.Confirmed)
                && e.end.isBefore()
                && moment(e.end).add('hours', 2).isAfter()
                && angular.isNumber(e.price);
        }

    } // end of class App.Sessions_Learner.Session

} // end of module
