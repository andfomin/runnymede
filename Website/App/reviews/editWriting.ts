module app.reviews {

    //export class EditWriting extends app.reviews.EditCtrlBase {

    //    static $inject = [app.ngNames.$appRemarks, app.ngNames.$http, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$q,
    //        app.ngNames.$rootScope, app.ngNames.$scope, app.ngNames.$window];

    //    constructor(
    //        $appRemarks: app.exercises.IRemarksService,
    //        $http: angular.IHttpService,
    //        $interval: angular.IIntervalService,
    //        $modal: angular.ui.bootstrap.IModalService,
    //        $q: angular.IQService,
    //        $rootScope: angular.IRootScopeService,
    //        $scope: app.IScopeWithViewModel,
    //        $window: angular.IWindowService
    //        )
    //    /* ----- Constructor  ------------ */
    //    {
    //        super($appRemarks, $http, $interval, $modal, $q, $rootScope, $scope, $window);
    //    }
    //    /* ----- End of constructor  ----- */
    //} // end of class EditWriting

    export class CanvasEditor extends app.exercises.Canvas {

        review: IReview;

        static $inject = [app.ngNames.$appRemarks, app.ngNames.$document, app.ngNames.$modal, app.ngNames.$scope, app.ngNames.$window];

        constructor(
            $appRemarks: app.exercises.IRemarksService,
            $document: angular.IDocumentService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            $window: angular.IWindowService
            )
        /* ----- Constructor  ------------ */
        {
            super($appRemarks, $document, $modal, $scope, $window);

            this.review = this.exercise.reviews[0];
        }
        /* ----- End of constructor  ----- */

        onCanvasClick = (event: MouseEvent) => {
            var point = this.relativeCoordinates(event);
            var remark = this.findRemark(point);
            if (remark) {
                this.selectRemark(remark);
            }
            else {
                if (this.canAddRemark()) {
                    remark = this.addRemark(point);
                    this.selectRemark(remark);
                }
            }
            this.$scope.$apply();
        };

        canAddRemark = () => {
            return !this.review.finishTime;
        };

        private addRemark = (point: app.exercises.IPoint) => {
            var remark = <IRemark>{
                id: app.reviews.getPieceId(this.review),
                reviewId: this.review.id,
                type: app.exercises.PieceTypes.Remark,
                page: this.page,
                x: point.x,
                y: point.y,
                like: false,
            };
            this.$appRemarks.add(remark);
            return remark;
        };

    } // end of class CanvasEditor

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .value(app.ngNames.$appRemarksComparer, app.exercises.WritingsComparer)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .controller('Canvas', app.reviews.CanvasEditor) // vma
        .controller('EditWriting', app.reviews.EditCtrlBase)
    ;

} // end of module app.reviews

