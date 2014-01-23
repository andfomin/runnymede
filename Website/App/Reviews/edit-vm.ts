module App.Reviews_Edit {

    export class ViewModel extends App.Reviews.ViewModelBase {

        static minSpotLength = 1000; // msec

        shiftSpot: (data: App.Model.IRemark, event: any) => any;
        addRemarkCmd: KoliteCommand;
        showFinishDialogCmd: KoliteCommand;
        finishCmd: KoliteCommand;
        openTagDialog: () => any;
        deleteRemark: (remark: App.Model.IRemark) => any;
        deleteCmd: KoliteCommand;
        dialogRemark: App.Model.IRemark;
        autoSaved: KnockoutObservable<boolean> = ko.observable(false);
        tagValue: KnockoutObservable<string> = ko.observable();
        tagValue2: KnockoutObservable<string> = ko.observable();

        constructor() {
            super();

            this.savingSourceName = 'FromEdit';

            this.setupAutoSave();
            this.setupOnBeforeUnload();

            this.saveMapper = (remarks) => {
                return $.map(remarks, (i) => {
                    return {
                        id: i.id,
                        reviewId: i.reviewId,
                        start: i.start,
                        finish: i.finish,
                        tags: i.tags,
                        text: i.text,
                    };
                });
                return remarks;
            };

            // overriden
            this.internalOnDirty = () => {
                this.autoSaved(false);
            };

            // overriden
            this.internalOnSaveDone = () => {
                if ($('#saveButton').hasClass('on-page-exit')) {
                    $('#saveButton').removeClass('on-page-exit');
                }
            };

            this.onRemarkSelected = (remark: App.Model.Remark) => {
                var element = $('#tagInput');
                element.detach();

                if (remark) {
                    $('tr.edited-remark > td.text-column > textarea').focus();

                    $('tr.edited-remark > td.tag-column > div.not-edited-hidden > div.tag-input-placeholder').append(element);
                    //element.show();
                    this.tagValue(remark.tags());
                }
                else {
                    //element.hide();
                    $('#editor').append(element);
                    this.tagValue();
                }
            };

            this.shiftSpot = (data, event) => {
                var className = event.target.className;

                if (className.indexOf('button-start-rewind') >= 0) {
                    this.internalShiftSpot(true, -1000);
                } else if (className.indexOf('button-start-forward') >= 0) {
                    this.internalShiftSpot(true, 1000);
                } else if (className.indexOf('button-finish-rewind') >= 0) {
                    this.internalShiftSpot(false, -1000);
                } else if (className.indexOf('button-finish-forward') >= 0) {
                    this.internalShiftSpot(false, 1000);
                }
            };

            this.addRemarkCmd = ko.asyncCommand({
                execute: (complete) => {
                    this.addRemark();
                    complete();
                },
                canExecute: (isExecuting) => {

                    return !isExecuting
                        && !this.selectedRemark()
                        && this.soundPosition() > 0
                        && this.soundPosition() < this.exercise().length;
                        //&& !this.review.finishTime();
                }
            });

            this.deleteRemark = (remark) => {
                this.sound.stop();
                this.dialogRemark = remark;
                (<any>$('#dialogDelete')).modal();
            };

            this.deleteCmd = ko.asyncCommand({
                execute: (complete) => {
                    App.Utils.ajaxRequest("DELETE", App.Utils.remarksApiUrl(this.dialogRemark.reviewId + '/' + this.dialogRemark.id))
                        .done(() => {
                            this.selectRemark(null);
                            this.remarks.remove(this.dialogRemark);
                            toastr.success('Remark deleted.');
                        })
                        .fail(() => { toastr.error('Error deleting remark.'); })
                        .always(() => {
                            this.dialogRemark = null;
                            complete();
                            (<any>$('#dialogDelete')).modal('hide');
                        });
                },
                canExecute: (isExecuting) => {
                    return !isExecuting;
                }
            });

            this.showFinishDialogCmd = ko.asyncCommand({
                execute: (complete) => {
                    this.turnPlayer(false);
                    (<any>$('#dialogFinish')).modal();
                    complete();
                },
                canExecute: (isExecuting) => {
                    return !isExecuting
                        && !this.review.finishTime()
                        && this.remarks().length > 0;
                }
            })

            this.finishCmd = ko.asyncCommand({
                execute: (complete) => {

                    var wrapup = (success: boolean) => {
                        complete();
                        (<any>$('#dialogFinish')).modal('hide');
                        if (success) {
                            toastr.success('Review finished');
                        }
                        else {
                            toastr.error('Error finishing review.');
                        }
                    }

                    var action = (saved: boolean) => {
                        if (saved) {
                            App.Utils.ajaxRequest("POST",
                                App.Utils.reviewsApiUrl(this.review.id + '/Finish'))
                                .done((data) => {
                                    this.review.finishTime(App.Utils.formatDateLocal(data.finishTime));
                                    wrapup(true);
                                })
                                .fail(() => { wrapup(false); });
                        }
                        else {
                            wrapup(false);
                        }
                    };

                    if (this.saveCmd.canExecute()) {
                        this.saveCmd.execute(action);
                    }
                    else {
                        action(true);
                    }
                },
                canExecute: (isExecuting) => {
                    return !isExecuting;
                }
            })

            this.getRemarkRowClass = (remark) => {
                var selectedClass;
                if (remark === this.selectedRemark()) {
                    //selectedClass = this.review.finishTime() ? 'selected-not-edited-remark' : 'edited-remark';
                    selectedClass = 'edited-remark';
                }
                else {
                    selectedClass = 'not-selected-remark';
                }
                var textClass = remark.text() || remark.tags() ? '' : ' warning';
                return selectedClass + textClass;
            }

            this.internalOnBeforeUnload = (event) => {
                if (this.saveCmd.canExecute()) {
                    $('#saveButton').addClass('on-page-exit');
                    var text = 'If you leave this page, your unsaved changes will be lost.';
                    if (event) {
                        event.returnValue = text;
                    }
                    return text;
                }
            };

            this.tagValue.subscribe((newValue) => {
                this.tagValue2(newValue);
                var r = this.selectedRemark();
                if (r) {
                    r.tags(newValue);
                }
            });

        } // end of ctor

        addRemark() {
            var finish = Math.max(ViewModel.minSpotLength, Math.floor(this.sound.position / 1000) * 1000);
            var start = Math.max(0, finish - 2 * ViewModel.minSpotLength);
            // Prevent duplicates
            var duplicate = ko.utils.arrayFirst(this.remarks(), (i) => {
                return (Math.abs(i.start() - start) < 500) && (Math.abs(i.finish() - finish) < 500);
            });

            if (!duplicate) {
                // Make Id sequecial. The clustered index in Azure Table is PartitionKey+RowKey.
                var base36Number = Math.floor((start + finish) / 2).toString(36).toUpperCase();
                // Make Id fixed-length.
                var remarkId = ('000000' + base36Number).substr(-6);

                this.remarks.push(new App.Model.Remark(
                    {
                        id: remarkId,
                        reviewId: this.review.id,
                        start: start,
                        finish: finish,
                        accepted: true
                    }));

                this.sortRemarks();
            }
        }

        internalShiftSpot(shiftStart: boolean, shift: number) {
            var r, sp, fp, nsp, nfp;
            r = this.selectedRemark();
            sp = r.start();
            fp = r.finish();

            var totalLength = this.exercise().length;
            var minLength = ViewModel.minSpotLength;

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

        tagButtonClickHadler(event) {
            var tag = (<HTMLElement>event.target).id;
            var remark = this.selectedRemark();
            if (tag && remark) {
                var t = remark.tags() || '';
                if (t.indexOf(tag) === -1) {
                    remark.tags(t + (t.length ? ', ' : '') + tag);
                }
            }
        }

        // overriden
        setupControls() {
            super.setupControls();

            //$('#tagListMenu').hide();
            //var tags = $('ul.mbmenu > li > a[id^="BCEG/"]');
            //tags.attr('href', 'javascript:;');
            //tags.click((event) => {
            //    this.tagButtonClickHadler(event);
            //    return true;
            //});
        }

        setupAutoSave() {

            var callback = (success) => {
                if (success) {
                    this.autoSaved(true);
                }
            };

            window.setInterval(() => {
                // Avoid confusion on returning after an exit attempt, let the user to save manually.
                if (this.saveCmd.canExecute() && !$('#saveButton').hasClass('on-page-exit')) {
                    this.saveCmd.execute(callback);
                }
            }
                , 60000);
        }

        setupOnBeforeUnload() {
            window.onbeforeunload = (event) => {
                if (this.saveCmd.canExecute()) {
                    $('#saveButton').addClass('on-page-exit');
                    var text = 'If you leave this page, your unsaved changes will be lost.';
                    if (event) {
                        event.returnValue = text;
                    }
                    return text;
                }
            };
        }

    } // end of class

}

$(() => {
    var vm = new App.Reviews_Edit.ViewModel();
    ko.applyBindings(vm);

    var element = $('#tagInput'); 
    element.hide();   
    var options = {
        source: (<any>App).TagList,
        items: 'all',
        minLength: 3,
    };
    (<any>element).typeahead(options);
    
});

