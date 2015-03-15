module app.reviews {

    export class EditCtrlBase extends app.exercises.CtrlBase {

        review: app.IReview; // The review being edited.
        isSaving: boolean = false;
        autoSaved: boolean = false;
        unsavedOnExit: boolean = false;
        categories: any[];

        static $inject = [app.ngNames.$appRemarks, app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$q,
            app.ngNames.$rootScope, app.ngNames.$scope, app.ngNames.$window];

        constructor(
            $appRemarks: app.exercises.IRemarksService,
            $http: angular.IHttpService,
            public $interval: angular.IIntervalService,
            private $modal: angular.ui.bootstrap.IModalService,
            private $q: angular.IQService,
            private $rootScope: angular.IRootScopeService,
            $scope: app.IScopeWithViewModel,
            public $window: angular.IWindowService
            )
        /* ----- Constructor  ------------ */ {
            super($appRemarks, $http, $scope);

            this.review = this.exercise.reviews[0];

            this.setupOnBeforeUnload();

            var autoSaveInterval = this.$interval(
                () => { this.autoSave(); },
                10000);

            $scope.$on('$destroy',() => {
                if (angular.isDefined(autoSaveInterval)) {
                    $interval.cancel(autoSaveInterval);
                }
                this.$window.onbeforeunload = undefined;
            });

            $scope.$on(app.exercises.RemarksService.remarksChanged,() => { this.autoSaved = false; });

            this.categories = app.library.Categories
                .filter((i) => { return i.pathIds.indexOf(app.library.ID_OF_PROGRAMS_CATEGORY) === -1; }) // Filter out everything under "Programs"
                .map((i) => { return { id: i.id, name: i.name } });
        }
        /* ----- End of constructor  ----- */

        setupOnBeforeUnload = () => {
            this.$window.onbeforeunload = (event) => {
                if (this.canSave()) {
                    // Highlight the Save button
                    this.unsavedOnExit = true;
                    this.$scope.$apply();
                    var text = 'Your unsaved changes will be lost if you leave this page.';
                    if (typeof event == 'undefined') {
                        event = window.event;
                    }
                    if (event) {
                        event.returnValue = text;
                    }
                    return text;
                }
            };
        }

        canAddSuggestion = () => {
            return !this.review.suggestions.some((i) => { return !i.suggestion && !i.keywords; }) && !this.review.finishTime;
        }

        addSuggestion = () => {
            var s = <app.ISuggestion> {
                id: getPieceId(this.review),
                reviewId: this.review.id,
                type: app.exercises.PieceTypes.Suggestion,
            };
            this.review.suggestions.push(s);
        }

        deleteSuggestion = (suggestion: app.ISuggestion) => {
            var partitionKey = getPiecePartitionKey(this.exercise);
            var rowKey = getPieceRowKey(suggestion);

            return this.$http.delete(
                app.reviewsApiUrl('piece/' + partitionKey + '/' + rowKey))
                .success(() => {
                // The operation is idempotent. An attempt to delete an unsaved item is no problem as well. In this case, just delete the item on the client.
                app.arrRemove(this.review.suggestions, suggestion);
                toastr.success('Suggestion was deleted.');
            });
        }

        isNotFinished = () => {
            return !this.review.finishTime;
        }

        makeDirty = (piece: app.IPiece) => {
            piece.dirtyTime = new Date();
            this.autoSaved = false;
        };

        canSave = () => {
            var remarkDirty = this.$appRemarks.remarks.some((i) => { return !!i.dirtyTime; });
            var suggestionDirty = this.review.suggestions.some((i) => { return !!i.dirtyTime; });
            var commentDirty = !!this.review.comment.dirtyTime;
            return (remarkDirty || suggestionDirty || commentDirty) && !this.isSaving;
        };

        save = () => {
            if (this.canSave()) {
                this.saveDirtyPieces()
                    .then(() => { this.unsavedOnExit = false; });
            }
        };

        autoSave = () => {
            if (this.canSave() && !this.unsavedOnExit) {
                this.saveDirtyPieces()
                    .then(() => { this.autoSaved = true; });
            }
        };

        saveDirtyPieces = () => {

            this.isSaving = true;
            // A remark or suggestion may get edited during the AJAX request. We save the moment and use it later when clearing the dirty flags.
            var now = new Date();

            var remarks = this.$appRemarks.remarks.filter((i) => { return !!i.dirtyTime; });

            var suggestions = this.review.suggestions.filter((i) => { return !!i.dirtyTime; });
            // Try to assign categoryId. Although we expect a category, ISuggestion.keywords may be any text. 
            suggestions.forEach((i: app.ISuggestion) => {
                var keywords = (i.keywords || '').trim();
                var category = app.arrFind(this.categories,(i) => { return i.name === keywords; });
                i.categoryId = category ? category.id : null;
            });

            var comments = this.review.comment.dirtyTime ? [this.review.comment] : [];

            var pieces: app.IPiece[] = [].concat(remarks, suggestions, comments);

            var promise: angular.IPromise<any> = null;

            if (pieces.length > 0) {
                // Make Runnymede.Website.Models.ReviewPiece entities to store them in Azure Table as is.
                var partitionKey = getPiecePartitionKey(this.exercise);

                var reviewPieces = pieces.map((i) => {
                    var clone = angular.copy(i);
                    // Remove redundand 40 chars per piece. Save on traffic.
                    //clone.dirtyTime = undefined; 
                    delete clone.dirtyTime;
                    var reviewPiece = {
                        partitionKey: partitionKey,
                        rowKey: getPieceRowKey(i),
                        json: angular.toJson(clone),
                    };
                    return reviewPiece;
                });

                promise = app.ngHttpPut(this.$http,
                    app.reviewsApiUrl('pieces'),
                    reviewPieces,
                    () => {
                        pieces.forEach((i) => {
                            if (i.dirtyTime < now) {
                                i.dirtyTime = null;
                            }
                        });
                    },
                    () => { this.isSaving = false; }
                    )
                    .catch((reason) => {
                    toastr.warning('Changes cannot be saved. Please reload the page.');
                    // .catch() does not propagate the original rejected promise (.then() doesn't either.) It returns a new resolved promise by default.
                    var d1 = this.$q.defer();
                    d1.reject(reason);
                    return d1.promise;
                });
            }
            else {
                var d2 = this.$q.defer();
                d2.reject();
                promise = d2.promise;
                this.isSaving = false;
            }

            return promise;
        };

        private unselectRemark = () => {
            this.$rootScope.$broadcast(app.exercises.RemarksService.unselectRemark, null);
        };

        showDeleteRemarkModal = (remark: IRemark) => {
            app.Modal.openModal(this.$modal,
                '/app/reviews/deleteRemarkModal.html',
                DeleteRemarkModal,
                {
                    partitionKey: getPiecePartitionKey(this.exercise),
                    rowKey: getPieceRowKey(remark),
                },
                () => {
                    this.unselectRemark();
                    this.$appRemarks.deleteRemark(remark);
                    toastr.success('Remark was deleted.');
                }
                )
        };

        showFinishReviewModal = () => {
            this.unselectRemark();

            var openModal = () => {
                app.Modal.openModal(this.$modal,
                    '/app/reviews/finishReviewModal.html',
                    FinishReviewModal,
                    {
                        review: this.review,
                    }
                    )
            };

            if (this.canSave()) {
                this.saveDirtyPieces()
                    .then(() => {
                    this.autoSaved = true;
                    openModal();
                });
            }
            else {
                openModal();
            }
        };

    } // end of class EditCtrlBase

    export class DeleteRemarkModal extends app.Modal {
        internalOk = () => {
            var partitionKey = this.modalParams.partitionKey;
            var rowKey = this.modalParams.rowKey;

            return this.$http.delete(
                app.reviewsApiUrl('piece/' + partitionKey + '/' + rowKey))
                .error(app.logError);
        };
    }; // end of class DeleteRemarkModal

    export class FinishReviewModal extends app.Modal {
        internalOk = () => {
            var review = this.modalParams.review;
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(review.id + '/finish'),
                null,
                (data) => {
                    review.finishTime = new Date(data.finishTime);
                    toastr.success('Review is finished.');
                },
                null
                );
        };
    }; // end of class FinishReviewModal

    export function getPieceId(review: IReview) {
        // The number means milliseconds passed from the start of the review. It is an uniquefier, not a meaningful time.
        return Date.now() - new Date(review.startTime).getTime();
    }

    export function getPiecePartitionKey(exercise: IExercise) {
        // Corresponds to ReviewPiece.GetPartitionKey()
        return app.intToKey(exercise.id);
    };

    export function getPieceRowKey(piece: IPiece) {
        // Corresponds to ReviewPiece.GetRowKey()
        return app.intToKey(piece.reviewId) + piece.type + app.intToKey(piece.id);
    };

} // end of module app.reviews

