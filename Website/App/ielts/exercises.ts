module app.ielts {

    export class Exercises extends app.CtrlBase {

        exercises: app.IExercise[];

        static $inject = [app.ngNames.$filter, app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope];

        constructor(
            private $filter: angular.IFilterService,
            private $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            private $modal: angular.ui.bootstrap.IModalService,
            private $scope: app.IScopeWithViewModel
            ) {
            super($scope);

            this.pgLoad();
            var refresher = $interval(() => { this.pgLoad(); }, 300000);
            $scope.$on('$destroy',() => { $interval.cancel(refresher); });

        } // end of ctor

        pgLoad = () => {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.exercisesApiUrl(),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit,
                    },
                    (data) => {
                        this.exercises = data.items;
                        this.pgTotal = data.totalCount;
                    }
                    );
            }
        }

        isEmpty() {
            return this.authenticated && this.pgTotal === 0;
        }

        getViewUrl(e: IExercise) {
            return '/exercises/view/' + e.id;
        }

        getReviewStatusText(r: app.IReview) {
            return r.finishTime ? 'Finished' : (r.startTime ? 'Started' : 'Requested');
        }

        getReviewStatusTime(r: app.IReview) {
            return r.finishTime ? r.finishTime : (r.startTime ? r.startTime : r.requestTime);
        }

        getReviewStatusStyle(r: app.IReview) {
            return r.finishTime ? 'text-success' : (r.startTime ? 'text-warning' : null);
        }

        showCreateRequestModal = (exercise: app.IExercise) => {
          app.exercises.showCreateRequestModal(this.$modal, exercise);
        };
    }

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Exercises', Exercises);

} // end of module

