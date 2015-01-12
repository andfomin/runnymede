module app.exercises {

    export class PieceTypes {
        // "E" editor user, "V" viewer user. Corresponds to Runnymede.Website.Models.ReviewPiece.PieceTypes
        public static Remark = 'R';
        public static Suggestion = 'S';
        public static Comment = 'C';
    }

    export class CtrlBase {

        exercise: app.IExercise;
        reviews: app.IReview[];

        constructor(
            public $appRemarks: app.exercises.IRemarksService,
            public $http: ng.IHttpService,
            public $scope: app.IScopeWithViewModel
            )
        /* ----- Constructor  ------------ */
        {
            $scope.vm = this;

            this.exercise = app['exerciseParam'];
            this.reviews = this.exercise.reviews;

            // Dummy pieces will be replaced as the real ones are loaded.
            this.reviews.forEach((i) => {
                i.suggestions = [];
                i.comment = <any>{
                    reviewId: i.id,
                    type: PieceTypes.Comment,
                    id: 1,
                    dirtyTime: null,
                };
            });

            this.loadPieces();
        }
        /* ----- End of constructor  ----- */

        loadPieces = () => {
            if (this.reviews.length > 0) {
                // We do not send exercise.createTime to the review editor. Use that fact to distinguish between the Review and Exercise pages.
                var route = ('exercise/' + this.exercise.id) + (this.exercise.createTime ? '' : ('/review/' + this.reviews[0].id)) + '/pieces';

                app.ngHttpGet(this.$http,
                    app.reviewsApiUrl(route),
                    null,
                    (data) => {
                        if (angular.isArray(data)) {
                            var pieces: app.IPiece[] = data.map((i) => { return angular.fromJson(i); });
                            var remarks = [];

                            pieces.forEach((i) => {
                                i.dirtyTime = null;
                                var review = app.arrFind(this.reviews, (r) => { return r.id === i.reviewId; });
                                if (review) {
                                    switch (i.type) {
                                        case PieceTypes.Remark:
                                            remarks.push(i);
                                            break;
                                        case PieceTypes.Suggestion:
                                            review.suggestions.push(<any>i);
                                            break;
                                        case PieceTypes.Comment:
                                            review.comment = <any>i;
                                            break;
                                    }
                                }
                            });

                            this.$appRemarks.setRemarks(remarks);
                        }
                    }
                    );
            }
        };

    } // end of class CtrlBase

    export interface IRemarksService {
        remarks: IRemark[];
        setRemarks: (remarks: IRemark[]) => void;
        add: (remark: IRemark) => void;
        deleteRemark: (remark: IRemark) => void;
        sort: () => void;
    }

    export class RemarksService implements IRemarksService {

        remarks: IRemark[] = [];

        static remarksChanged = 'remarksService.remarksChanged';
        static unselectRemark = 'remarksService.unselectRemark';

        static $inject = [app.ngNames.$appRemarksComparer, app.ngNames.$rootScope];

        constructor(
            private $appRemarksComparer: (a: IRemark, b: IRemark) => number,
            private $rootScope: ng.IRootScopeService
            ) { /* ----- Ctor  ----- */
        } // end of ctor

        setRemarks = (remarks: IRemark[]) => {
            this.remarks = remarks;
            this.sort();
            this.notify();
        };

        add = (remark: IRemark) => {
            remark.dirtyTime = new Date();
            this.remarks.push(remark);
            this.sort();
            this.notify();
        };

        deleteRemark = (remark) => {
            app.arrRemove(this.remarks, remark);
            this.notify();
        };

        sort = () => {
            this.remarks.sort(this.$appRemarksComparer);
        };

        private notify = () => {
            this.$rootScope.$broadcast(RemarksService.remarksChanged, null);
        };

    } // end of class RemarksService


} // end of module app.exercises
