module App.Reviews {

    export class Edit extends App.Reviews.CtrlBase {

        static minSpotLength = 1000; // msec

        isDirty: boolean = false;
        isSaving: boolean = false;
        autoSaved: boolean = false;


        constructor(
            $scope: Utils.IScopeWithViewModel,
            $http: ng.IHttpService
            ) {
            super($scope, $http);
            $scope.vm = this;



        } // end of ctor

        save() {

        }

        canAddRemark() {
            return (!this.selectedRemark)
                && (this.soundPosition > 0)
                && (this.soundPosition < this.exercise.length);
        }

        addRemark() {
            var finish = Math.max(Edit.minSpotLength, Math.floor(this.sound.position / 1000) * 1000 - 1000); // -1000 msec for human reaction delay.
            var start = Math.max(0, finish - Edit.minSpotLength);
            // Prevent duplicates
            var duplicate = false;
            angular.forEach(this.remarks, (i) => {
                if ((Math.abs(i.start - start) < 500) && (Math.abs(i.finish - finish) < 500)) {
                    duplicate = true;
                }
            });
            if (!duplicate) {
                // Make Id sequencial. The clustered index in the Azure Table is PartitionKey+RowKey.
                var base36Number = Math.floor((start + finish) / 2).toString(36).toUpperCase();
                // Make Id fixed-length.
                var remarkId = ('000000' + base36Number).substr(-6);

                this.remarks.push({
                    id: remarkId,
                    reviewId: this.review.id,
                    start: start,
                    finish: finish,
                    tags: null,
                    text: null,
                    dirty: true
                });

                this.sortRemarks();
            }
        }

        getRemarkRowClass(remark) {
            var selectedClass;
            if (remark === this.selectedRemark) {
                //selectedClass = this.review.finishTime() ? 'selected-not-edited-remark' : 'edited-remark';
                selectedClass = 'edited-remark';
            }
            else {
                selectedClass = 'not-selected-remark';
            }
            var textClass = remark.text || remark.tags ? '' : ' warning';
            return selectedClass + textClass;
        }

        shiftSpot(shiftStart: boolean, shift: number) {
            var r, sp, fp, nsp, nfp;
            r = this.selectedRemark;
            sp = r.start();
            fp = r.finish();

            var totalLength = this.exercise.length;
            var minLength = Edit.minSpotLength;

            if (shiftStart) {
                nsp = Math.max(0, Math.min(totalLength - minLength, sp + shift));
                nfp = Math.max(fp, Math.min(totalLength, nsp + minLength));
            }
            else {
                nfp = Math.max(0 + minLength, Math.min(totalLength, fp + shift));
                nsp = Math.max(0, Math.min(sp, nfp - minLength));
            }

            r.start(nsp);
            r.finish(nfp);

            this.sortRemarks();

            this.playRemarkSpot(r);
        }




    } // end of class

} // end of module

var app = angular.module('app', ['AppUtilsNg', 'chieffancypants.loadingBar']);
app.controller('Edit', App.Reviews.Edit);
