var App;
(function (App) {
    ////module App {
    ////    // Passed via a script in the HTML
    ////    declare var exerciseParam: any;
    ////    declare var soundUrlParam: string;
    ////}
    (function (Reviews) {
        var ViewModelBase = (function () {
            function ViewModelBase() {
                var _this = this;
                this.sound = null;
                this.remarks = ko.observableArray([]);
                this.soundPosition = ko.observable(0);
                this.selectedRemark = ko.observable();
                this.dirtyAutoSaveInterval = 0;
                this.exercise = ko.observable(new App.Model.Exercise((App).exerciseParam));
                this.review = this.exercise().reviews()[0];

                this.loadRemarks();
                this.setupControls();
                this.setupOnBeforeUnload();

                soundCreator.done(function () {
                    _this.createSound();
                });

                this.isDirty = ko.computed(function () {
                    return ko.utils.arrayFirst(_this.remarks(), function (i) {
                        return i.dirtyFlag().isDirty();
                    }) != null;
                });

                this.isDirty.subscribe(function (newValue) {
                    if (newValue) {
                        _this.internalOnDirty();
                    }
                });

                //this.internalOnDirty = () => {/* To be overriden in descendants. */ };
                //this.internalOnSaveDone = () => {/* To be overriden in descendants. */ };
                this.saveCmd = ko.asyncCommand({
                    execute: function (saveOrigin, complete) {
                        $.when(_this.saveDirtyRemarks(saveOrigin)).always(complete);
                    },
                    canExecute: function (isExecuting) {
                        var res = !isExecuting && _this.isDirty();
                        return res;
                    }
                });

                this.onRemarkSelected = function (remark) {
                };

                this.selectRemark = function (remark, event) {
                    if (typeof event === "undefined") { event = null; }
                    var old = _this.selectedRemark();
                    _this.selectedRemark(remark);

                    if ((remark !== old) || !event) {
                        _this.onRemarkSelected(remark);
                        if (remark) {
                            _this.playRemarkSpot(remark);
                        }
                    }

                    // +http://knockoutjs.com/documentation/click-binding.html#note_3_allowing_the_default_click_action
                    return true;
                };

                this.createSound = function () {
                    _this.sound = window['soundManager'].createSound({
                        id: 'mySound',
                        url: (App).soundUrlParam,
                        autoLoad: true,
                        multiShot: false,
                        onload: function (success) {
                            if (success) {
                                $('#editor').show();
                            } else {
                                toastr.error('Error loading the sound file.');
                            }
                        },
                        whileplaying: function () {
                            ($('#slider')).slider("value", _this.sound.position);
                            _this.soundPosition(_this.sound.position);
                        },
                        onfinish: function () {
                            _this.turnPlayer(false);
                        }
                    });
                };

                this.formattedPosition = ko.computed(function () {
                    return App.Utils.formatMsec(_this.soundPosition());
                });

                this.turnPlayer = function (play) {
                    var button = $('#playButton');
                    if (play) {
                        if (_this.sound.paused) {
                            _this.sound.resume();
                        } else {
                            _this.sound.play();
                        }
                        button.addClass('active');
                        _this.selectRemark(null);
                    } else {
                        if (_this.sound.playState === 1) {
                            _this.sound.pause();
                        }
                        button.removeClass('active');
                    }
                    button.children('i').removeClass('fa-pause fa-play').addClass(play ? 'fa-pause' : 'fa-play');
                    button.children('span').text(play ? 'Pause' : 'Play');
                };

                this.playerClick = function () {
                    _this.turnPlayer(!$('#playButton').hasClass('active'));
                };

                this.playRemarkSpot = function (remark) {
                    _this.sound.stop();
                    _this.sound.play({ from: remark.start(), to: remark.finish() });
                };

                this.getRemarkRowClass = function (remark) {
                };

                this.getSpotMarkClass = function (remark) {
                    var selectedClass = remark === _this.selectedRemark() ? 'selected-mark' : '';
                    var bottomClass = remark.reviewId === _this.review.id ? '' : ' bottom-mark';
                    return selectedClass + bottomClass;
                };
            }
            ViewModelBase.prototype.sortRemarks = function () {
                this.remarks.sort(function (left, right) {
                    var l = left.start() + left.finish(), r = right.start() + right.finish();
                    return l === r ? 0 : (l < r ? -1 : 1);
                });
            };

            // The folowing logic supports multiple possible requests and thus is able to combine remarks from multiple reviews on the same page.
            ViewModelBase.prototype.loadRemarks = function () {
                var _this = this;
                var requests = $.map(this.exercise().reviews(), function (i) {
                    return App.Utils.ajaxRequest('GET', App.Utils.remarksApiUrl('Review/' + i.id));
                });

                if (requests.length > 0) {
                    // Show an NETEYE Activity Indicator.
                    ($(document.body)).activity();

                    $.when.apply($, requests).done(function () {
                        var dtoArr = [];

                        if (requests.length > 1) {
                            $.each(arguments, function (i, arg) {
                                dtoArr = dtoArr.concat(arg[0]);
                            });
                        } else {
                            dtoArr = dtoArr.concat(arguments[0]);
                        }

                        $.each(dtoArr, function (i, dto) {
                            var r = new App.Model.Remark(dto);
                            r.dirtyFlag().reset();
                            _this.remarks().push(r);
                        });

                        _this.sortRemarks();
                    }).fail(function () {
                        toastr.error('Error loading remarks.');
                    }).always(function () {
                        ($(document.body)).activity(false);
                    });
                }
            };

            ViewModelBase.prototype.rewind = function () {
                var newPosition = Math.max(this.sound.position - 5000, 0);
                this.sound.setPosition(newPosition);
                ($('#slider')).slider("value", newPosition);
                this.soundPosition(newPosition);

                this.selectRemark(null);
            };

            ViewModelBase.prototype.onSliderChange = function (event, ui) {
                if (event.originalEvent) {
                    this.sound.setPosition(ui.value);
                    this.soundPosition(ui.value);
                    this.turnPlayer(true);
                }
            };

            ViewModelBase.prototype.onSliderSlide = function (event, ui) {
                if (event.originalEvent) {
                    this.soundPosition(ui.value);
                    this.turnPlayer(false);
                }
            };

            ViewModelBase.prototype.setupControls = function () {
                var _this = this;
                $('#editor').hide();

                ($('#slider')).slider({
                    min: 0,
                    max: this.exercise().length,
                    change: function (event, ui) {
                        return _this.onSliderChange(event, ui);
                    },
                    slide: function (event, ui) {
                        return _this.onSliderSlide(event, ui);
                    }
                });
            };

            ViewModelBase.prototype.saveDirtyRemarks = function (arg) {
                var _this = this;
                var remarksToSave = ko.utils.arrayFilter(this.remarks(), function (i) {
                    return i.dirtyFlag().isDirty();
                });

                // A remark may being edited at the moment of the ajax request.
                ko.utils.arrayForEach(remarksToSave, function (i) {
                    i.dirtyFlag().reset();
                });

                return App.Utils.ajaxRequest('PUT', App.Utils.remarksApiUrl(this.savingSourceName), this.saveMapper(remarksToSave)).done(function () {
                    if (typeof arg == typeof Function) {
                        arg(true);
                    } else {
                        if (arg && (typeof arg == 'object') && arg.hasOwnProperty('internalOnSaveDone')) {
                            _this.internalOnSaveDone();
                        }
                    }
                    _this.dirtyAutoSaveInterval = 0;
                }).fail(function (jqXHR) {
                    // Possible error causes:
                    // 1. The review time expired and the review has been finished forcefully on the server.
                    // 2. The authorization cookie expired and the user has to relogin.
                    // 3. The user pulled the plug :-)
                    toastr.error('Error saving remarks.');

                    if (jqXHR.status === 400) {
                        toastr.warning('The page will be reloaded in 5 seconds.');
                        window.setTimeout(function () {
                            window.location.reload(true);
                        }, 5000);
                    } else {
                        _this.dirtyAutoSaveInterval = _this.dirtyAutoSaveInterval === 0 ? 2000 : _this.dirtyAutoSaveInterval * 2;

                        // Return remarks to the dirty state on unknown failure to continue saving attempts.
                        ko.utils.arrayForEach(remarksToSave, function (i) {
                            var temp = i.start();
                            i.start(null);
                            i.dirtyFlag().reset();
                            i.start(temp);
                        });
                    }

                    if (typeof arg == typeof Function) {
                        arg(false);
                    }
                });
            };

            ViewModelBase.prototype.getSpotMarkStyleLeft = function (remark) {
                // percent
                return 50 * (remark.start() + remark.finish()) / this.exercise().length;
            };

            ViewModelBase.prototype.setupOnBeforeUnload = function () {
                var _this = this;
                window.onbeforeunload = function (event) {
                    _this.internalOnBeforeUnload(event);
                };
            };
            return ViewModelBase;
        })();
        Reviews.ViewModelBase = ViewModelBase;

        // +http://stackoverflow.com/questions/8611327/integrating-jquery-ui-dialog-with-knockoutjs
        //ko.bindingHandlers['dialog'] = {
        //    init: function (element, valueAccessor, allBindingsAccessor) {
        //        var options = ko.utils.unwrapObservable(valueAccessor()) || {};
        //        //do in a setTimeout, so the applyBindings doesn't bind twice from element being copied and moved to bottom
        //        setTimeout(function () {
        //            options.close = function () {
        //                allBindingsAccessor().dialogVisible(false);
        //            };
        //            $(element).dialog(options);
        //        }, 0);
        //        //handle disposal (not strictly necessary in this scenario)
        //        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
        //            $(element).dialog("destroy");
        //        });
        //    },
        //    update: function (element, valueAccessor, allBindingsAccessor) {
        //        var shouldBeOpen = ko.utils.unwrapObservable(allBindingsAccessor().dialogVisible),
        //            $el = $(element),
        //            dialog = $el.data("uiDialog") || $el.data("dialog");
        //        //don't call open/close before initilization
        //        if (dialog) {
        //            $el.dialog(shouldBeOpen ? "open" : "close");
        //        }
        //    }
        //};
        var soundCreator = $.Deferred();

        //  SoundManager must be initialized before DOM onload. Otherwise browser security may block Flash <-> JS communication after certain JQuery DOM manipulations by a third-party code.
        window['soundManager'].setup({
            url: '/Scripts/sm2/',
            preferFlash: true,
            flashVersion: 9,
            useFlashBlock: false,
            debugMode: false,
            onready: function () {
                soundCreator.resolve();
            },
            ontimeout: function () {
                toastr.error('Error. Audio player could not start.');
            }
        });
    })(App.Reviews || (App.Reviews = {}));
    var Reviews = App.Reviews;
})(App || (App = {}));
//# sourceMappingURL=vm-base.js.map
