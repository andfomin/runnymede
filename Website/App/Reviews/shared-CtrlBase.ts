module App.Reviews {

    export class CtrlBase {

        sound: any = null;

        exercise: App.Model.IExercise2;
        reviews: App.Model.IReview2[];
        remarks: App.Model.IRemark2[];
        review: App.Model.IReview2; // The edited review / selected review.
        selectedRemark: App.Model.IRemark2 = null;
        soundPosition: number = 0;



        saveCmd: KoliteCommand;
        internalOnSaveDone: () => void;
        savingSourceName: string;
        saveMapper: (remarks: App.Model.IRemark[]) => any[];
        createSound: () => void;
        onRemarkSelected: (remark: App.Model.IRemark) => void;
        turnPlayer: (play: boolean) => void;
        getRemarkRowClass: (remark: App.Model.IRemark) => void;
        internalOnDirty: () => void;
        internalOnBeforeUnload: (event: BeforeUnloadEvent) => string;
        dirtyAutoSaveInterval: number = 0; //msec


        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {

            this.exercise = (<any>App).exerciseParam;
            this.reviews = this.exercise.reviews;
            this.review = this.reviews[0];


        } // end of ctor

        selectRemark(remark: App.Model.IRemark2) {

        }

        getSpotMarkStyleLeft(remark) {
            // percent
            return 50 * (remark.start + remark.finish) / this.exercise.length;
        }

        getSpotMarkClass(remark) {
            var selectedClass = remark === this.selectedRemark ? 'selected-mark' : '';
            var bottomClass = remark.reviewId === this.review.id ? '' : ' bottom-mark';
            return selectedClass + bottomClass;
        }

        rewind() {
            var newPosition = Math.max(this.sound.position - 5000, 0);
            this.sound.setPosition(newPosition);
            (<JQueryUI.UI><any>$('#slider')).slider("value", newPosition);
            this.soundPosition = newPosition;
            this.selectRemark = null;
        }

        playerClick() {
            this.turnPlayer(!$('#playButton').hasClass('active'));
        }

        sortRemarks() {
            this.remarks.sort(
                function (left, right) {
                    var l = left.start + left.finish;
                    var r = right.start + right.finish;
                    return l === r ? 0 : (l < r ? -1 : 1);
                });
        }

        playRemarkSpot(remark) {
            this.sound.stop();
            this.sound.play({
                from: remark.start,
                to: remark.finish
            });
        }




    } // end of class
} // end of module
