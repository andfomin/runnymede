module app.exercises {

    export class PieceTypes {
        // Corresponds to Runnymede.Website.Models.ReviewPiece.PieceTypes in Runnymede\Common\Models\ExerciseModels.cs
        // "E" - editor user, "V" - viewer user.
        public static Remark = 'R';
        public static Performance = 'P';
        public static Suggestion = 'S';
        public static Comment = 'C';
        public static Video = 'Y';
    }

    export class CtrlBase {

        exercise: app.IExercise;
        card: app.ICard;
        reviews: app.IReview[];
        descGroups: app.exercisesIelts.IDescriptor[][][]; // criterion area / descriptor group / descriptor 

        static PiecesLoaded = 'piecesLoaded';

        constructor(
            public $appRemarks: app.exercises.IRemarksService,
            public $http: angular.IHttpService,
            public $rootScope: angular.IRootScopeService,
            public $scope: app.IScopeWithViewModel,
            cardProvider: ILazyProvider<ICard>
        )
        /* ----- Constructor  ------------ */ {
            $scope.vm = this;

            this.exercise = app['exerciseParam'];
            this.reviews = this.exercise.reviews;

            cardProvider.get()
                .then((card) => {
                    this.card = card;
                });

            // Performance descriptors. Flat array filtered by serviceType.
            var descs: app.exercisesIelts.IDescriptor[] =
                []
                    .concat(app.exercisesIelts.descsW1, app.exercisesIelts.descsW2, app.exercisesIelts.descsS_)
                    .filter((i) => { return i.path.substr(0, 6) === this.exercise.serviceType; });
            // Parse path.
            descs.forEach((i) => {
                i.group = i.path.substr(0, 11);
                i.band = +i.path.substr(12, 1);
            });
            // First group by criterion area
            var groups: app.exercisesIelts.IDescriptor[][] = app.arrGroupBy(descs, (i) => { return i.path.substr(0, 9); });
            // Then group by descriptor group
            this.descGroups = groups.map((i) => {
                return app.arrGroupBy(i, (j) => { return j.group; });
            });

            // The dummy pieces will be replaced as the real ones have been loaded.
            this.reviews.forEach((i) => {
                i.performance = this.createPiece(i.id, PieceTypes.Performance);
                i.suggestions = [];
                i.comment = this.createPiece(i.id, PieceTypes.Comment);
                i.video = this.createPiece(i.id, PieceTypes.Video);
            });

            this.loadPieces();
        }
        /* ----- End of constructor  ----- */

        // RowKey includes the piece's Type. We can use the same id with different types.
        private createPiece = (reviewId: number, type: string) => {
            return <any>{
                reviewId: reviewId,
                type: type,
                id: 1,
                dirtyTime: null,
            };
        };

        loadPieces = () => {
            if (this.reviews.length > 0) {
                // We do not send exercise.creationTime to the review editor. Use that fact to distinguish between the Review and Exercise pages.
                var singleReview = !this.exercise.creationTime;
                var route = ('exercise/' + this.exercise.id) + (singleReview ? ('/review/' + this.reviews[0].id) : '') + '/pieces';

                app.ngHttpGet(this.$http,
                    app.reviewsApiUrl(route),
                    null,
                    (data) => {
                        this.updatePieces(data);
                        this.$rootScope.$broadcast(CtrlBase.PiecesLoaded);
                    });
            }
        };

        updatePieces = (data: any) => {
            if (angular.isArray(data)) {
                var pieces: app.IPiece[] = data.map((i) => { return angular.fromJson(i); });
                var remarks = [];

                pieces.forEach((i) => {
                    i.dirtyTime = null;
                    var reviewId = i.reviewId;
                    var review = app.arrFind(this.reviews, (r) => { return r.id === reviewId; });
                    if (review) {
                        switch (i.type) {
                            case PieceTypes.Remark:
                                remarks.push(i);
                                break;
                            case PieceTypes.Performance:
                                review.performance = <any>i;
                                break;
                            case PieceTypes.Suggestion:
                                var ss = review.suggestions;
                                var s = app.arrFind(ss, (j) => { return j.id === i.id; });
                                if (s) {
                                    ss[ss.indexOf(s)] = <any>i;
                                }
                                else {
                                    ss.push(<any>i);
                                }
                                break;
                            case PieceTypes.Comment:
                                review.comment = <any>i;
                                break;
                            case PieceTypes.Video:
                                review.video = <any>i;
                                break;
                        }
                    }
                });

                this.$appRemarks.upsertRemarks(remarks);
            }
        };

        getCriterionAreaTitle = (d: app.exercisesIelts.IDescriptor) => {
            return app.exercisesIelts.criterionAreaTitles[d.path.substr(0, 9)];
        };

    } // end of class CtrlBase

    export interface IRemarksService {
        remarks: IRemark[];
        upsertRemarks: (remarks: IRemark[]) => void;
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
            private $rootScope: angular.IRootScopeService
        ) { /* ----- Ctor  ----- */
        } // end of ctor

        upsertRemarks = (remarks: IRemark[]) => {
            remarks.forEach((i) => {
                var r = app.arrFind(this.remarks, (j) => { return (j.id === i.id) && (j.reviewId === i.reviewId); });
                if (r) {
                    this.remarks[this.remarks.indexOf(r)] = i;
                }
                else {
                    this.remarks.push(i);
                }
            });

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
            this.$rootScope.$broadcast(RemarksService.remarksChanged);
        };

    } // end of class RemarksService

    export class CardProvider implements ILazyProvider<ICard> {
        static ServiceName = 'appCardProvider';
        //static SupportedTypes = [ServiceType.IeltsWritingTask1, ServiceType.IeltsWritingTask2, ServiceType.IeltsSpeaking];

        promise: angular.IPromise<ICard>;

        static $inject = [app.ngNames.$http]
        constructor(
            public $http: angular.IHttpService
        ) {
            //this.serviceType = app['serviceTypeParam'];
            //if (IeltsBase.SupportedTypes.indexOf(this.serviceType) != -1) {
            //}
        };

        get = () => {
            if (!this.promise) {
                //var serviceType = app['serviceTypeParam'];
                //var url = app.exercisesApiUrl('user_card/' + serviceType);
                //var url = '/content/aec3cc24-92b4-4cc3-88d1-c235395e30d0.json';
                var cardId = app['cardIdParam'];
                var url = '/app/exercisesIelts/' + cardId + '.json';
                this.promise = app.ngHttpGet(this.$http, url, null, null)
                    .then(
                    (response: angular.IHttpPromiseCallbackArg<ICard>) => {
                        return response.data;
                    },
                    (reason) => { return { message: reason } }
                    );
            }
            return this.promise;
        };

    } // end of class CardProvider

} // end of module app.exercises
