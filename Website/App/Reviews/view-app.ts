module App.Reviews {

    export class Viewer extends App.Reviews.CtrlBase {

        originalTitle: string = null;
        reviewerNames: any = {};

        constructor(
            $scope: Utils.IScopeWithViewModel,
            $http: ng.IHttpService,
            $filter: ng.IFilterService,
            $interval: ng.IIntervalService,
            $modal: ng.ui.bootstrap.IModalService,
            $q: ng.IQService
            ) {
            /* ----- Ctor  ----- */
            super($scope, $http, $filter, $interval, $modal, $q);
            $scope.vm = this;

            this.originalTitle = this.exercise.title;

            // Create a map object for fast lookup of reviewerNames.
            this.reviews.forEach((i) => {
                this.reviewerNames[i.id.toString()] = i.reviewerName;
            });

            /* ----- End of ctor  ----- */
        }

        isTitleDirty = () => {
            return this.exercise.title !== this.originalTitle;
        }

        saveTitle = () => {
            var title = this.exercise.title.trim();
            if (title) {
                App.Utils.ngHttpPut(this.$http,
                    App.Utils.exercisesApiUrl(this.exercise.id + '/Title'),
                    {
                        title: title
                    },
                    () => {
                        this.exercise.title = title;
                        this.originalTitle = this.exercise.title;
                    }
                    )
            }
        }

        getReviewerName = (remark) => {
            return this.reviewerNames[remark.reviewId.toString()];
        }

    } // end of class Viewer

} // end of module

// The app module is initialized in shared-ctrlBase.ts
app.controller('Viewer', App.Reviews.Viewer);
