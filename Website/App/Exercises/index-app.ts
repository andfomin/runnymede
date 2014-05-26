module App.Exercises_Index {

    export interface IModalParams {
        review: App.Model.IReview2;
        exercise: App.Model.IExercise2;
    }

    export class Ctrl {

        exercises: App.Model.IExercise2[] = [];
        limit: number = 10; // items per page
        currentPage: number = 1;
        totalCount: number = 0;
        loaded: boolean; 

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$modal, App.Utils.ngNames.$interval, App.Utils.ngNames.$timeout];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            $interval: ng.IIntervalService,
            private $timeout: ng.ITimeoutService
            ) {
            $scope.vm = this;
            this.getExercises();
            $interval(() => { this.getExercises(); }, 300000);
        } // end of ctor

        getExercises() {
            App.Utils.ngHttpGetWithParamsNoCache(this.$http,
                App.Utils.exercisesApiUrl('GetExercises'),
                {
                    offset: (this.currentPage - 1) * this.limit,
                    limit: this.limit
                },
                (data) => {
                    this.exercises = data.items;
                    this.totalCount = data.totalCount;
                    this.loaded = true;
                }
                );
        }

        isEmpty() {
            return this.loaded && this.exercises && this.exercises.length == 0;
        }

        getViewUrl(e: App.Model.IExercise2) {
            return App.Utils.reviewsUrl('view/' + e.id);
        }

        getReviewStatusText(r: App.Model.IReview2) {
            return r.finishTime ? 'Finished' : (r.startTime ? 'Started' : (r.cancelTime ? 'Canceled' : 'Requested'));
        }

        getReviewStatusTime(r: App.Model.IReview2) {
            return r.finishTime ? r.finishTime : (r.startTime ? r.startTime : (r.cancelTime ? r.cancelTime : r.requestTime));
        }

        getReviewStatusStyle(r: App.Model.IReview2) {
            return r.finishTime ? 'text-success' : (r.startTime ? 'text-info' : (r.cancelTime ? 'text-muted' : null));
        }

        showCreateRequestModal = (exercise) => {
            App.Utils.CustomModal.openModal(
                this.$modal,
                'createRequestDialog.html',
                CreateRequestModal,
                {
                    exercise: exercise
                },
                () => { } // this.getExercises() is redundand. API returns the review object.
                )
                .opened.then(() => {
                    // Set input focus to the reward field. But first wait for css animated transitions to complete.
                    this.$timeout(() => {
                        $('#reward1').focus(); // It needs the jQuery, jqLite does not support focus() (does it?)
                    }, 200);
                });
        };

        showCancelRequestModal = (review: App.Model.IReview2) => {
            // Find the exercise for the given review.
            var exerciseId = review.exerciseId;
            var exercise = App.Utils.find(
                this.exercises,
                (el) => { return el.id === exerciseId; }
                );

            App.Utils.CustomModal.openModal(
                this.$modal,
                'cancelRequestDialog.html',
                CancelRequestModal,
                {
                    review: review,
                    exercise: exercise,
                },
                () => { this.getExercises(); }
                )
        };
    } // end of class Ctrl

    export class CreateRequestModal extends App.Utils.CustomModal {

        workDurationRatio: number; // Average ratio of work duration to exercise length. It is used for calculation of a suggested reward.
        balance: number;
        teachers: App.Model.IUser[] = [];
        anyReviewer: boolean = true;
        reward1: string;
        reward2: string;
        selectedTeacher: App.Model.IUser = null;
        exercise: App.Model.IExercise2;

        constructor(
            $scope: App.Utils.IScopeWithViewModel,
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            modalParams: any
            ) {
            super($scope, $http, $modalInstance, modalParams);
            this.exercise = modalParams.exercise;
            this.getConditions();
        } // ctor

        getConditions() {
            // Request review conditions from the server.
            App.Utils.ngHttpGetWithParamsNoCache(this.$http,
                App.Utils.exercisesApiUrl(this.exercise.id.toString() + '/ReviewConditions'),
                null,
                (data) => {
                    this.workDurationRatio = data.workDurationRatio;
                    this.balance = 0 + data.balance;
                    this.teachers = data.teachers;
                });
        }

        suggestedReward() {
            var reward = this.selectedTeacher
                ? this.selectedTeacher.reviewRate * this.exercise.length * this.workDurationRatio / 3600000 // MSec to Hour
                : 0;
            return Math.round(reward * 100) / 100; // round to cents.
        }

        disabled() {
            var ok = App.Utils.isValidAmount(this.reward1)
                && (Number(this.reward1.replace(',', '.')) > 0) // The API method accepts the ',' separator as well.
                && (this.reward2 === this.reward1)
                && (this.anyReviewer || this.selectedTeacher);
            return !ok;
        }

        internalOk = () => {
            return App.Utils.ngHttpPost(this.$http,
                App.Utils.reviewsApiUrl(),
                {
                    exerciseId: this.exercise.id,
                    reward: this.reward1,
                    // The current version of GUI allows for selection of a single teacher only. The backend is able to accept an array of teachers.
                    teachers: (!this.anyReviewer && this.selectedTeacher) ? [this.selectedTeacher.id] : [],
                },
                (data) => {
                    this.exercise.reviews.push(data);
                    toastr.success('Review requested.');
                });
        };
    }; // end of class CreateRequestModal

    export class CancelRequestModal extends App.Utils.CustomModal {

        internalOk = () => {
            return this.$http.delete(
                App.Utils.reviewsApiUrl(this.modalParams.review.id.toString())
                )
                .success(() => {
                    toastr.success('Review request canceled');
                });
        };
    }; // end of class CancelRequestModal


} // end of module

var app = angular.module('app', ['AppUtilsNg', 'ui.bootstrap', 'chieffancypants.loadingBar']);
app.controller('Ctrl', App.Exercises_Index.Ctrl);
