module app.exercises {

    export class ViewCtrlBase extends app.exercises.CtrlBase {

        private originalTitle: string = null;
        private selectedSuggestion: ISuggestion = null;
        private reviewerNames: any = {};
        private busy: boolean;

        static $inject = [app.ngNames.$appRemarks, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$rootScope, app.ngNames.$scope];

        constructor(
            $appRemarks: app.exercises.IRemarksService,
            $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            private $rootScope: ng.IRootScopeService,
            public $scope: app.IScopeWithViewModel
            )
        /* ----- Constructor  ------------ */
        {
            super($appRemarks, $http, $scope);

            this.originalTitle = this.exercise.title;

            // Create a dictionary object for fast lookup of reviewerNames.
            this.reviews.forEach((i) => {
                this.reviewerNames[i.id.toString()] = i.reviewerName;
            });
        }
        /* ----- End of constructor  ----- */

        isTitleDirty = () => {
            return this.exercise.title !== this.originalTitle;
        }

        saveTitle = () => {
            var title = this.exercise.title.trim();
            if (title) {
                app.ngHttpPut(this.$http,
                    app.exercisesApiUrl(this.exercise.id + '/title'),
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

        manyReviews = () => {
            return this.reviews.length > 1;
        }

        selectSuggestion = (suggestion: app.ISuggestion) => {
            if (this.selectedSuggestion !== suggestion) {
                this.selectedSuggestion = suggestion;

                this.$rootScope.$broadcast(app.library.ResourceList.Clear);
                //var dummyResource: app.ISimpleResource = {
                //    id: null,
                //    url: '+https://www.google.com/#q=' + this.selectedSuggestion.keywords,
                //    title: 'Google',
                //};
                if (suggestion && !this.busy) {
                    this.busy = true;
                    app.ngHttpGet(this.$http,
                        app.libraryApiUrl('common'),
                        {
                            categoryId: suggestion.categoryId,
                            q: suggestion.keywords,
                        },
                        (data) => {
                            if (data && angular.isArray(data.value)) {
                                this.$rootScope.$broadcast(app.library.ResourceList.Display, { resources: data.value });
                            }
                        },
                        () => { this.busy = false; }
                        );
                }
            }
        };

        showCreateRequestModal = () => {
            showCreateRequestModal(this.$modal, this.exercise);
        };
    }
}