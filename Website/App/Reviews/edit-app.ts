module App.Reviews {

    export class Editor extends App.Reviews.CtrlBase {

        static minSpotLength = 2000; // msec
        review: App.Model.IReview2; // The edited review / selected review.
        isSaving: boolean = false;
        autoSaved: boolean = false;
        unsavedOnExit: boolean = false;
        commentDirty: boolean = false;
        coreInventory: string[];

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

            this.review = new App.Model.Review2(this.reviews[0]);
            this.coreInventory = App.CoreInventory;

            if (this.review.suggestions.length === 0 && !this.review.finishTime) {
                this.addSuggestion();
            }

            this.setupOnBeforeUnload();

            var autoSaveInterval = this.setupAutoSave();
            $scope.$on('$destroy', () => {
                $interval.cancel(autoSaveInterval);
                window.onbeforeunload = undefined;
            });

            /* ----- End of ctor  ----- */
        }

        setupOnBeforeUnload = () => {
            window.onbeforeunload = (event) => {
                if (this.canSave()) {
                    // Highlight the Save button
                    this.$scope.$apply(() => {
                        this.unsavedOnExit = true;
                    });
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

        setupAutoSave = () => {
            var autoSaveInterval = this.$interval(() => {
                if (this.canSave() && !this.unsavedOnExit) {
                    this.saveDirtyItems()
                        .then(() => { this.autoSaved = true; });
                }
            }, 60000);
            return autoSaveInterval;
        }

        save = () => {
            if (this.canSave()) {
                this.saveDirtyItems()
                    .then(() => { this.unsavedOnExit = false; });
            }
        }

        canSave = () => {
            var remarkDirty = this.remarks.some((i) => { return i.dirtyTime !== null; });    // !this.remarks.every((i) => { return i.dirtyTime === null; });
            var suggestionDirty = this.review.suggestions.some((i) => { return i.dirtyTime !== null });
            var commentDirty = this.commentDirty;
            return (remarkDirty || suggestionDirty || commentDirty) && !this.isSaving;
        }

        canAddRemark = () => {
            return (!this.selectedRemark)
                && (this.soundPosition > 0)
                && (this.soundPosition < this.exercise.length);
        }

        addRemark = () => {
            var finish = Math.max(Editor.minSpotLength, Math.floor((this.sound.position - 500) / 1000) * 1000); // -1000 msec for human reaction delay. +500 for better rounding
            var start = Math.max(0, finish - Editor.minSpotLength);
            var remark: App.Model.IRemark2 = {
                reviewId: this.review.id,
                creationTime: this.getCreationTime(),
                start: start,
                finish: finish,
                text: null,
                keywords: null,
                dirtyTime: null,
            };
            this.makeRemarkDirty(remark);
            this.remarks.push(remark);
            this.sortRemarks();
        }

        canAddSuggestion = () => {
            return !(this.review.suggestions.filter((i) => { return !i.text; }).length);
        }

        addSuggestion = () => {
            var s: App.Model.ISuggestion = {
                reviewId: this.review.id,
                creationTime: this.getCreationTime(),
                text: null,
                dirtyTime: null
            };
            this.review.suggestions.push(s);
        }

        deleteSuggestion = (suggestion) => {
            return this.$http.delete(App.Utils.reviewsApiUrl('Suggestion/' + suggestion.reviewId + '/' + suggestion.creationTime))
                .success(() => {
                    App.Utils.arrRemove(this.review.suggestions, suggestion);
                });
        }

        getCreationTime = () => {
            return new Date().getTime() - this.review.startTime.getTime();
        }

        isHighlighted = (remark) => {
            return !remark.text && !remark.keywords && !this.review.finishTime;
        }

        isEdited = (remark) => {
            return (remark === this.selectedRemark) && !this.review.finishTime;
        }

        shiftSpot = (remark: App.Model.IRemark2, shiftStart: boolean, shift: number) => {
            var os = remark.start;
            var of = remark.finish;

            var totalLength = this.exercise.length;
            var minLength = Editor.minSpotLength;

            var ns, nf;
            if (shiftStart) {
                ns = Math.max(0, Math.min(totalLength - minLength, os + shift));
                nf = Math.max(of, Math.min(totalLength, ns + minLength));
            }
            else {
                nf = Math.max(0 + minLength, Math.min(totalLength, of + shift));
                ns = Math.max(0, Math.min(os, nf - minLength));
            }

            remark.start = ns;
            remark.finish = nf;
            this.makeRemarkDirty(remark);
            this.sortRemarks();
            this.playRemarkSpot(remark);
        }

        saveDirtyItems = () => {
            this.isSaving = true;
            // A remark or suggestion may get edited during the AJAX request. We save the moment and use it later when clearing the dirty flags.
            var now = new Date();

            var remarksCall: ng.IPromise<any> = null;
            var remarksToSave = this.remarks.filter((i) => { return i.dirtyTime !== null; });
            if (remarksToSave.length > 0) {
                remarksCall = App.Utils.ngHttpPut(this.$http,
                    App.Utils.remarksApiUrl(),
                    remarksToSave,
                    () => {
                        remarksToSave.forEach((i: App.Model.IRemark2) => {
                            if (i.dirtyTime < now) {
                                i.dirtyTime = null;
                            }
                        });
                    }
                    );
            };

            var suggestionsCall: ng.IPromise<any> = null;
            var suggestionsToSave = this.review.suggestions.filter((i) => { return i.dirtyTime !== null; });
            if (suggestionsToSave.length > 0) {
                suggestionsCall = App.Utils.ngHttpPut(this.$http,
                    App.Utils.reviewsApiUrl('Suggestions'),
                    suggestionsToSave,
                    () => {
                        suggestionsToSave.forEach((i: App.Model.ISuggestion) => {
                            if (i.dirtyTime < now) {
                                i.dirtyTime = null;
                            }
                        });
                    }
                    );
            };

            var commentCall: ng.IPromise<any> = null;
            if (this.commentDirty) {
                commentCall = App.Utils.ngHttpPut(this.$http,
                    App.Utils.reviewsApiUrl('Comment/' + this.review.id),
                    {
                        comment: this.review.comment
                    },
                    () => { this.commentDirty = false; }
                    );
            }

            return this.$q.all([remarksCall, suggestionsCall, commentCall])
                .then(null, () => { toastr.warning('Changes cannot be saved. Please reload the page.'); })
                .finally(() => { this.isSaving = false; })
        }

        makeRemarkDirty = (remark) => {
            remark.dirtyTime = new Date();
            this.autoSaved = false;
        }

        makeSuggestionDirty = (suggestion) => {
            suggestion.dirtyTime = new Date();
            this.autoSaved = false;
        }

        makeCommentDirty = () => {
            this.commentDirty = true;
            this.autoSaved = false;
        }

        showDeleteRemarkModal = (remark) => {
            App.Utils.CustomModal.openModal(
                this.$modal,
                'deleteRemarkModal.html',
                DeleteRemarkModal,
                {
                    remark: remark,
                },
                () => {
                    this.selectRemark(null);
                    App.Utils.arrRemove(this.remarks, remark);
                }
                )
        }

        showFinishReviewModal = () => {
            this.selectRemark(null);

            var openModal = () => {
                App.Utils.CustomModal.openModal(
                    this.$modal,
                    'finishReviewModal.html',
                    FinishReviewModal,
                    {
                        review: this.review,
                    },
                    null
                    )
            };

            if (this.canSave()) {
                this.saveDirtyItems()
                    .then(() => {
                        this.autoSaved = true;
                        openModal();
                    });
            }
            else {
                openModal();
            }
        }

    } // end of class Editor

    export class DeleteRemarkModal extends App.Utils.CustomModal {
        internalOk = () => {
            var remark = this.modalParams.remark;
            return this.$http.delete(App.Utils.remarksApiUrl(remark.reviewId + '/' + remark.creationTime))
                .success(() => {
                    toastr.success('Remark was deleted.');
                });
        };
    }; // end of class DeleteRemarkModal

    export class FinishReviewModal extends App.Utils.CustomModal {
        internalOk = () => {
            var review = this.modalParams.review;
            return App.Utils.ngHttpPost(this.$http,
                App.Utils.reviewsApiUrl('Finish/' + review.id),
                null,
                (data) => {
                    review.finishTime = new Date(data.finishTime);
                    toastr.success('Review is finished.');
                },
                null
                );
        };
    }; // end of class DeleteRemarkModal

} // end of module

// The app module is initialized in shared-ctrlBase.ts
app.controller('Editor', App.Reviews.Editor);
