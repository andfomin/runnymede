module app.exercises {

    export class ViewCtrlBase extends app.exercises.CtrlBase {

        private originalTitle: string = null;
        private originalLength: number = 0;
        private selectedSuggestion: ISuggestion = null;
        private reviewerNames: any = {};
        private busy: boolean;

        static $inject = [app.ngNames.$appRemarks, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$rootScope, app.ngNames.$scope, app.ngNames.$signalRService,
            app.ngNames.$timeout, app.ngNames.$window];

        constructor(
            $appRemarks: app.exercises.IRemarksService,
            $http: angular.IHttpService,
            private $modal: angular.ui.bootstrap.IModalService,
            $rootScope: angular.IRootScopeService,
            public $scope: app.IScopeWithViewModel,
            public $signalRService: app.ISignalRService,
            $timeout: angular.ITimeoutService,
            $window: angular.IWindowService
            )
        /* ----- Constructor  ------------ */ {
            super($appRemarks, $http, $rootScope, $scope);

            this.originalTitle = this.exercise.title;
            this.originalLength = this.exercise.length;

            // Create a dictionary object for fast lookup of reviewerNames.
            this.reviews.forEach((i) => {
                this.reviewerNames[i.id.toString()] = i.reviewerName;
            });

            $scope.$on(CtrlBase.PiecesLoaded,() => {
                // Wait until Angular inserts new elements in DOM.
                $timeout(() => {
                    this.reviews
                        .filter((i) => { return !!(i.video && i.video.url); })
                        .forEach((i) => {
                        var tagId = 'toBeReplacedByYoutubeIframe-' + i.id;
                        var elem = $window.document.getElementById(tagId);
                        // The Youtube script will replace DIV with IFRAME
                        if (elem.tagName === 'DIV') {
                            var videoId = app.getYtVideoId(i.video.url);
                            app.createYouTubePlayer($window, 960, 720, tagId,
                                (event: YT.EventArgs) => { event.target.cueVideoById(videoId); },
                                (event: YT.EventArgs) => { toastr.error('Player error ' + event.data); }
                                );
                        }
                    });
                }, 200);
            });

            this.$signalRService.setEventHandlers('reviewHub', [
                {
                    eventName: 'piecesChanged',
                    handler: (args: any[]) => {
                        this.updatePieces(args[0]);
                    },
                },
                {
                    eventName: 'pieceDeleted',
                    handler: (args: any[]) => {
                        this.onPieceDeleted(args[0], args[1], args[2]);
                    }
                },
                {
                    eventName: 'reviewStarted',
                    handler: (args: any[]) => {
                        this.onReviewStarted(args[0], args[1], args[2], args[3]);
                    }
                },
                {
                    eventName: 'reviewFinished',
                    handler: (args: any[]) => {
                        this.onReviewFinished(args[0], args[1]);
                    }
                }
            ]);

            this.controlWatcher();
        }
        /* ----- End of constructor  ----- */

        isTitleDirty = () => {
            return (this.exercise.title !== this.originalTitle) && !this.busy;
        }

        saveTitle = () => {
            var title = this.exercise.title.trim();
            if (title) {
                this.busy = true;
                app.ngHttpPut(this.$http,
                    app.exercisesApiUrl(this.exercise.id + '/title'),
                    {
                        title: title
                    },
                    () => {
                        this.exercise.title = title;
                        this.originalTitle = this.exercise.title;
                    },
                    () => { this.busy = false; }
                    )
            }
        }

        canEditLength = () => {
            return (this.exercise.artifactType === ArtifactType.Jpeg) && this.reviews.every((i) => { return !i.startTime; });
        }

        isLengthDirty = () => {
            return (+this.exercise.length !== this.originalLength) && !this.busy;
        }

        saveLength = () => {
            var length = +this.exercise.length;
            if (app.isNumber(length)) {
                this.busy = true;
                app.ngHttpPut(this.$http,
                    app.exercisesApiUrl(this.exercise.id + '/length'),
                    {
                        length: length
                    },
                    () => {
                        this.exercise.length = length;
                        this.originalLength = length;
                    },
                    () => { this.busy = false; }
                    )
            }
            else {
                toastr.error('Number expected');
            }
        }

        getReviewerName = (remark) => {
            return this.reviewerNames[remark.reviewId.toString()];
        }

        manyReviews = () => {
            return this.startedReviews().length > 1;
        }

        startedReviews = () => {
            return this.reviews.filter((i) => { return !!i.startTime; });
        };

        hasVideo = (r: IReview) => {
            return r.video && app.getYtVideoId(r.video.url);
        };

        selectSuggestion = (suggestion: app.ISuggestion) => {
            if (this.selectedSuggestion !== suggestion) {
                this.selectedSuggestion = suggestion;

                this.$rootScope.$broadcast(app.library.ResourceList.Clear);
                //var dummyResource: app.ISimpleResource = {
                //    id: null,
                //    url: '+https://www.google.com/#q=' + this.selectedSuggestion.keywords,
                //    title: 'Google',
                //};
                if (suggestion && (suggestion.categoryId || suggestion.keywords) && !this.busy) {
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
            showCreateRequestModal(this.$modal, this.exercise,() => { this.controlWatcher(); });
        };

        private controlWatcher = () => {
            var active = this.reviews.some((i) => { return !i.finishTime; });
            if (active) {
                var promise = this.$signalRService.start();
                if (promise) {
                    promise.done(() => {
                        // Register the watcher on first start. Affiliate the exercise watcher with the connection. ConnectionId is keept the same on reconnects.
                        // RegisterWatcher on the server is idempotent.
                        this.$signalRService.invoke('reviewHub', 'registerWatcher', this.exercise.id);
                    });
                }
            }
            else {
                this.$signalRService.stop();
            }
        };

        private onPieceDeleted = (reviewId: number, pieceType: string, pieceId: number) => {
            switch (pieceType) {
                case PieceTypes.Remark:
                    var r = app.arrFind(this.$appRemarks.remarks,(i) => { return (i.id === pieceId) && (i.reviewId === reviewId); });
                    if (r) {
                        this.$appRemarks.deleteRemark(r);
                    }
                    break;
                case PieceTypes.Suggestion:
                    this.reviews.forEach((i) => {
                        if (i.id === reviewId) {
                            var ss = i.suggestions;
                            var s = app.arrFind(ss,(j) => { return j.id === pieceId; });
                            if (s) {
                                app.arrRemove(ss, s);
                                this.$rootScope.$broadcast(app.library.ResourceList.Clear);
                            }
                        }
                    });
                    break;
            }
        };

        private onReviewStarted = (reviewId: number, time: any, userId: number, name: string) => {
            var review = app.arrFind(this.reviews,(i) => { return i.id === reviewId; });
            if (review) {
                review.startTime = time;
                review.userId = userId;
                review.reviewerName = name;
                this.showReviewStatusChangedModal(true);
            }
        };

        private onReviewFinished = (reviewId: number, time: any) => {
            var review = app.arrFind(this.reviews,(i) => { return i.id === reviewId; });
            if (review) {
                review.finishTime = time;
                this.controlWatcher();
                this.showReviewStatusChangedModal(false);
            }
        };

        private showReviewStatusChangedModal = (started: boolean) => {
            this.$modal.open({
                templateUrl: '/app/exercises/reviewStatusChangedModal.html',
                size: 'sm',
                windowClass: started ? 'my-started' : 'my-finished',
            });
        };

    } // end of class ViewCtrlBase

} // end of module app.exercises