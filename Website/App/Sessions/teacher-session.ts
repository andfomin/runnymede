module App.Sessions_Teacher {

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

        canConfirm = () => {
            return (this.event.type === App.Model.ScheduleEvent.Types.Requested) && this.event.start.isAfter();
        }

        canCancel = () => {
            return (this.event.type === App.Model.ScheduleEvent.Types.Requested) || (this.event.type === App.Model.ScheduleEvent.Types.Confirmed);
        }

    } // end of class
} // end of module
