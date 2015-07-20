module app.reviews {

    //export class EditRecording extends app.reviews.EditCtrlBase {

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

    //} // end of class EditRecording

    export class AudioPlayerEditor extends app.exercises.AudioPlayer {

        review: IReview;
        minSpotLength = 2000; // msec

        static $inject = [app.ngNames.$appRemarks, app.ngNames.$filter, app.ngNames.$interval, app.ngNames.$modal, app.ngNames.$scope, app.ngNames.$timeout];

        constructor(
            private $appRemarks: app.exercises.IRemarksService,
            $filter: angular.IFilterService,
            $interval: angular.IIntervalService,
            private $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            $timeout: angular.ITimeoutService
            )
        /* ----- Constructor  ------------ */
        {
            super($scope, $filter, $interval, $timeout);

            this.review = this.exercise.reviews[0];

        }
        /* ----- End of constructor  ----- */

        canAddRemark = () => {
            return (!this.remark)
                && (this.soundPosition > 0)
                && (this.soundPosition < this.exercise.length);
        }

        addRemark = () => {
            var finish = Math.max(this.minSpotLength, Math.floor((this.sound.position - 500) / 1000) * 1000); // -1000 msec for human reaction delay. +500 for better rounding
            var start = Math.max(0, finish - this.minSpotLength);
            var remark = <IRemark>{
                reviewId: this.review.id,
                type: app.exercises.PieceTypes.Remark,
                id: app.reviews.getPieceId(this.review),
                start: start,
                finish: finish,
            };
            this.$appRemarks.add(remark);
        }

        shiftSpot = (remark: app.IRemark, shiftStart: boolean, shift: number) => {
            var os = remark.start;
            var of = remark.finish;

            var totalLength = this.exercise.length;
            var minLength = this.minSpotLength;

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

            this.$appRemarks.sort();

            this.playRemarkSpot(remark);
        };

        isHighlighted = (remark: app.IRemark) => {
            return remark && !(remark.correction || remark.comment || this.review.finishTime);
        }

        isNotFinished = () => {
            return !this.review.finishTime;
        }

        isEditing = (remark: app.IRemark) => {
            var unfinished = this.exercise.reviews.some((i) => { return !i.finishTime; });
            return (remark === this.remark) && unfinished;
        }

    } // end of class AudioPlayerEditor

    angular.module(app.myAppName, [app.utilsNg, 'ngSanitize', 'ui.bootstrap', 'angular-loading-bar', 'vr.directives.slider'])
        .value(app.ngNames.$appRemarksComparer, app.exercises.RecordingsComparer)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .controller('AudioPlayer', app.reviews.AudioPlayerEditor) // vma
        .controller('EditRecording', app.reviews.EditCtrlBase)
    ;

} // end of module app.reviews


