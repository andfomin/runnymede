var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var App;
(function (App) {
    (function (Reviews_Edit) {
        var ViewModel = (function (_super) {
            __extends(ViewModel, _super);
            function ViewModel() {
                var _this = this;
                _super.call(this);
                this.autoSaved = ko.observable(false);
                this.tagValue = ko.observable();
                this.tagValue2 = ko.observable();

                this.savingSourceName = 'FromEdit';

                this.setupAutoSave();
                this.setupOnBeforeUnload();

                this.saveMapper = function (remarks) {
                    return $.map(remarks, function (i) {
                        return {
                            id: i.id,
                            reviewId: i.reviewId,
                            start: i.start,
                            finish: i.finish,
                            tags: i.tags,
                            text: i.text
                        };
                    });
                    return remarks;
                };

                // overriden
                this.internalOnDirty = function () {
                    _this.autoSaved(false);
                };

                // overriden
                this.internalOnSaveDone = function () {
                    if ($('#saveButton').hasClass('on-page-exit')) {
                        $('#saveButton').removeClass('on-page-exit');
                    }
                };

                this.onRemarkSelected = function (remark) {
                    var element = $('#tagInput');
                    element.detach();

                    if (remark) {
                        $('tr.edited-remark > td.text-column > textarea').focus();

                        $('tr.edited-remark > td.tag-column > div.not-edited-hidden > div.tag-input-placeholder').append(element);

                        //element.show();
                        _this.tagValue(remark.tags());
                    } else {
                        //element.hide();
                        $('#editor').append(element);
                        _this.tagValue();
                    }
                };

                this.shiftSpot = function (data, event) {
                    var className = event.target.className;

                    if (className.indexOf('button-start-rewind') >= 0) {
                        _this.internalShiftSpot(true, -1000);
                    } else if (className.indexOf('button-start-forward') >= 0) {
                        _this.internalShiftSpot(true, 1000);
                    } else if (className.indexOf('button-finish-rewind') >= 0) {
                        _this.internalShiftSpot(false, -1000);
                    } else if (className.indexOf('button-finish-forward') >= 0) {
                        _this.internalShiftSpot(false, 1000);
                    }
                };

                this.addRemarkCmd = ko.asyncCommand({
                    execute: function (complete) {
                        _this.addRemark();
                        complete();
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting && !_this.selectedRemark() && _this.soundPosition() > 0 && _this.soundPosition() < _this.exercise().length;
                        //&& !this.review.finishTime();
                    }
                });

                this.deleteRemark = function (remark) {
                    _this.sound.stop();
                    _this.dialogRemark = remark;
                    ($('#dialogDelete')).modal();
                };

                this.deleteCmd = ko.asyncCommand({
                    execute: function (complete) {
                        App.Utils.ajaxRequest("DELETE", App.Utils.remarksApiUrl(_this.dialogRemark.reviewId + '/' + _this.dialogRemark.id)).done(function () {
                            _this.selectRemark(null);
                            _this.remarks.remove(_this.dialogRemark);
                            toastr.success('Remark deleted.');
                        }).fail(function () {
                            toastr.error('Error deleting remark.');
                        }).always(function () {
                            _this.dialogRemark = null;
                            complete();
                            ($('#dialogDelete')).modal('hide');
                        });
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting;
                    }
                });

                this.showFinishDialogCmd = ko.asyncCommand({
                    execute: function (complete) {
                        _this.turnPlayer(false);
                        ($('#dialogFinish')).modal();
                        complete();
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting && !_this.review.finishTime() && _this.remarks().length > 0;
                    }
                });

                this.finishCmd = ko.asyncCommand({
                    execute: function (complete) {
                        var wrapup = function (success) {
                            complete();
                            ($('#dialogFinish')).modal('hide');
                            if (success) {
                                toastr.success('Review finished');
                            } else {
                                toastr.error('Error finishing review.');
                            }
                        };

                        var action = function (saved) {
                            if (saved) {
                                App.Utils.ajaxRequest("POST", App.Utils.reviewsApiUrl(_this.review.id + '/Finish')).done(function (data) {
                                    _this.review.finishTime(App.Utils.formatDateLocal(data.finishTime));
                                    wrapup(true);
                                }).fail(function () {
                                    wrapup(false);
                                });
                            } else {
                                wrapup(false);
                            }
                        };

                        if (_this.saveCmd.canExecute()) {
                            _this.saveCmd.execute(action);
                        } else {
                            action(true);
                        }
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting;
                    }
                });

                this.getRemarkRowClass = function (remark) {
                    var selectedClass;
                    if (remark === _this.selectedRemark()) {
                        //selectedClass = this.review.finishTime() ? 'selected-not-edited-remark' : 'edited-remark';
                        selectedClass = 'edited-remark';
                    } else {
                        selectedClass = 'not-selected-remark';
                    }
                    var textClass = remark.text() || remark.tags() ? '' : ' warning';
                    return selectedClass + textClass;
                };

                this.internalOnBeforeUnload = function (event) {
                    if (_this.saveCmd.canExecute()) {
                        $('#saveButton').addClass('on-page-exit');
                        var text = 'If you leave this page, your unsaved changes will be lost.';
                        if (event) {
                            event.returnValue = text;
                        }
                        return text;
                    }
                };

                this.tagValue.subscribe(function (newValue) {
                    _this.tagValue2(newValue);
                    var r = _this.selectedRemark();
                    if (r) {
                        r.tags(newValue);
                    }
                });
            }
            ViewModel.prototype.addRemark = function () {
                var finish = Math.max(ViewModel.minSpotLength, Math.floor(this.sound.position / 1000) * 1000);
                var start = Math.max(0, finish - 2 * ViewModel.minSpotLength);

                // Prevent duplicates
                var duplicate = ko.utils.arrayFirst(this.remarks(), function (i) {
                    return (Math.abs(i.start() - start) < 500) && (Math.abs(i.finish() - finish) < 500);
                });

                if (!duplicate) {
                    // Make Id sequecial. The clustered index in Azure Table is PartitionKey+RowKey.
                    var base36Number = Math.floor((start + finish) / 2).toString(36).toUpperCase();

                    // Make Id fixed-length.
                    var remarkId = ('000000' + base36Number).substr(-6);

                    this.remarks.push(new App.Model.Remark({
                        id: remarkId,
                        reviewId: this.review.id,
                        start: start,
                        finish: finish,
                        accepted: true
                    }));

                    this.sortRemarks();
                }
            };

            ViewModel.prototype.internalShiftSpot = function (shiftStart, shift) {
                var r, sp, fp, nsp, nfp;
                r = this.selectedRemark();
                sp = r.start();
                fp = r.finish();

                var totalLength = this.exercise().length;
                var minLength = ViewModel.minSpotLength;

                if (shiftStart) {
                    nsp = Math.max(0, Math.min(totalLength - minLength, sp + shift));
                    nfp = Math.max(fp, Math.min(totalLength, nsp + minLength));
                } else {
                    nfp = Math.max(0 + minLength, Math.min(totalLength, fp + shift));
                    nsp = Math.max(0, Math.min(sp, nfp - minLength));
                }

                r.start(nsp);
                r.finish(nfp);

                this.sortRemarks();

                this.playRemarkSpot(r);
            };

            ViewModel.prototype.tagButtonClickHadler = function (event) {
                var tag = (event.target).id;
                var remark = this.selectedRemark();
                if (tag && remark) {
                    var t = remark.tags() || '';
                    if (t.indexOf(tag) === -1) {
                        remark.tags(t + (t.length ? ', ' : '') + tag);
                    }
                }
            };

            // overriden
            ViewModel.prototype.setupControls = function () {
                _super.prototype.setupControls.call(this);
                //$('#tagListMenu').hide();
                //var tags = $('ul.mbmenu > li > a[id^="BCEG/"]');
                //tags.attr('href', 'javascript:;');
                //tags.click((event) => {
                //    this.tagButtonClickHadler(event);
                //    return true;
                //});
            };

            ViewModel.prototype.setupAutoSave = function () {
                var _this = this;
                var callback = function (success) {
                    if (success) {
                        _this.autoSaved(true);
                    }
                };

                window.setInterval(function () {
                    if (_this.saveCmd.canExecute() && !$('#saveButton').hasClass('on-page-exit')) {
                        _this.saveCmd.execute(callback);
                    }
                }, 60000);
            };

            ViewModel.prototype.setupOnBeforeUnload = function () {
                var _this = this;
                window.onbeforeunload = function (event) {
                    if (_this.saveCmd.canExecute()) {
                        $('#saveButton').addClass('on-page-exit');
                        var text = 'If you leave this page, your unsaved changes will be lost.';
                        if (event) {
                            event.returnValue = text;
                        }
                        return text;
                    }
                };
            };
            ViewModel.minSpotLength = 1000;
            return ViewModel;
        })(App.Reviews.ViewModelBase);
        Reviews_Edit.ViewModel = ViewModel;
    })(App.Reviews_Edit || (App.Reviews_Edit = {}));
    var Reviews_Edit = App.Reviews_Edit;
})(App || (App = {}));

$(function () {
    var vm = new App.Reviews_Edit.ViewModel();
    ko.applyBindings(vm);

    var element = $('#tagInput');
    element.hide();
    var options = {
        source: (App).TagList,
        items: 'all',
        minLength: 3
    };
    (element).typeahead(options);
});
//# sourceMappingURL=edit-vm.js.map
