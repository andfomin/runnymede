module app.exercises {

    export class Index extends app.CtrlBase {

        exercises: app.IExercise[];

        static $inject = [app.ngNames.$filter, app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$rootScope, app.ngNames.$scope, app.ngNames.$state, app.ngNames.$timeout];

        constructor(
            private $filter: ng.IFilterService,
            private $http: ng.IHttpService,
            $interval: ng.IIntervalService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $rootScope: ng.IRootScopeService,
            private $scope: app.IScopeWithViewModel,
            private $state: ng.ui.IStateService,
            private $timeout: ng.ITimeoutService
            ) {
            super($scope);

            var refresher = $interval(() => { this.pgLoad(); }, 300000);
            $scope.$on('$destroy', () => { $interval.cancel(refresher); });

            $rootScope.$on('$stateChangeSuccess', (event, toState, toParams, fromState, fromParams) => { this.pgLoad(); });

        } // end of ctor

        pgLoad = () => {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.exercisesApiUrl(),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit,
                        type: this.$state.current.name,
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
            return r.finishTime ? 'Finished' : (r.startTime ? 'Started' : (r.cancelationTime ? 'Canceled' : 'Requested'));
        }

        getReviewStatusTime(r: app.IReview) {
            return r.finishTime ? r.finishTime : (r.startTime ? r.startTime : (r.cancelationTime ? r.cancelationTime : r.requestTime));
        }

        getReviewStatusStyle(r: app.IReview) {
            return r.finishTime ? 'text-success' : (r.startTime ? 'text-warning' : (r.cancelationTime ? 'text-muted' : null));
        }

        isStateSpeaking = () => {
            return this.$state.current.name === ExerciseType.AudioRecording;
        };

        isStateWriting = () => {
            return this.$state.current.name === ExerciseType.WritingPhoto;
        };

        showCreateRequestModal = (exercise: app.IExercise) => {
            showCreateRequestModal(this.$modal, exercise);
        };

        showCancelRequestModal = (review: app.IReview) => {
            // Find the exercise for the given review.
            var exerciseId = review.exerciseId;
            var exercise = app.arrFind(
                this.exercises,
                (el) => { return el.id === exerciseId; }
                );

            app.Modal.openModal(this.$modal,
                'cancelRequestModal',
                CancelRequestModal,
                {
                    review: review,
                    exercise: exercise,
                },
                () => { this.pgLoad(); }
                )
        };
    } // end of class Ctrl      

    export class CancelRequestModal extends app.Modal {

        internalOk = () => {
            return this.$http.delete(
                app.reviewsApiUrl(this.modalParams.review.id.toString())
                )
                .success(() => {
                    toastr.success('Review request canceled');
                })
                .error(app.logError);
        };
    }; // end of class CancelRequestModal

    class StateConfig {
        static $inject = [app.ngNames.$stateProvider, app.ngNames.$urlRouterProvider];
        constructor(private $stateProvider: ng.ui.IStateProvider, private $urlRouterProvider: ng.ui.IUrlRouterProvider) {
            var makeState = (name: string, title: string, secTitle: string) => {
                return {
                    name: name,
                    url: '/' + title,
                    data: {
                        secondaryTitle: secTitle,
                    },
                };
            };
            // The state names are used as URL path in pgLoad(). They correspond to routes in Runnymede.Website.Controllers.Api.LibraryApiController
            var speaking: ng.ui.IState = makeState(ExerciseType.AudioRecording, 'speaking', 'Audio recordings of speeches and conversations');
            var writing: ng.ui.IState = makeState(ExerciseType.WritingPhoto, 'writing', 'Photos of handwritten essays');
            $stateProvider
                .state(speaking)
                .state(writing)
            ;
            $urlRouterProvider.otherwise(speaking.url);
        }
    }; // end of class StateConfig

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'ui.router', 'angular-loading-bar'])
        .config(StateConfig)
        .run(app.StateTitleSyncer)
        .controller('Index', Index);

} // end of module

