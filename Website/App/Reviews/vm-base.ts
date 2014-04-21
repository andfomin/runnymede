////module App {
////    // Passed via a script in the HTML
////    declare var exerciseParam: any;
////    declare var soundUrlParam: string;
////}

module App.Reviews {

    export class ViewModelBase {

        sound: any = null;
        exercise: KnockoutObservable<App.Model.IExercise>;
        review: App.Model.IReview; // The single review.
        remarks: KnockoutObservableArray<App.Model.IRemark> = ko.observableArray([]);
        soundPosition: KnockoutObservable<number> = ko.observable(0);
        selectedRemark: KnockoutObservable<App.Model.IRemark> = ko.observable(null);
        saveCmd: KoliteCommand;
        internalOnSaveDone: () => void;
        isDirty: KnockoutComputed<boolean>;
        selectRemark: (remark: App.Model.Remark, event?: any) => void;
        savingSourceName: string;
        saveMapper: (remarks: App.Model.IRemark[]) => any[];
        createSound: () => void;
        onRemarkSelected: (remark: App.Model.IRemark) => void;
        formattedPosition: KnockoutComputed<string>;
        turnPlayer: (play: boolean) => void;
        playerClick: () => void;
        playRemarkSpot: (remark: App.Model.IRemark) => void;
        getRemarkRowClass: (remark: App.Model.IRemark) => void;
        getSpotMarkClass: (remark: App.Model.IRemark) => void;
        internalOnDirty: () => void;
        internalOnBeforeUnload: (event: BeforeUnloadEvent) => string;
        dirtyAutoSaveInterval: number = 0; //msec

        constructor() {
            this.exercise = ko.observable(new App.Model.Exercise((<any>App).exerciseParam));
            this.review = this.exercise().reviews()[0];

            this.loadRemarks();
            this.setupControls();
            this.setupOnBeforeUnload();

            soundCreator.done(() => { this.createSound(); });

            this.isDirty = ko.computed(() => {
                return ko.utils.arrayFirst(this.remarks(), (i) => { return i.dirtyFlag().isDirty(); }) != null;
            });

            this.isDirty.subscribe((newValue) => {
                if (newValue) {
                    this.internalOnDirty();
                }
            });

            //this.internalOnDirty = () => {/* To be overriden in descendants. */ };

            //this.internalOnSaveDone = () => {/* To be overriden in descendants. */ };

            this.saveCmd = ko.asyncCommand({
                execute: (saveOrigin, complete) => {
                    $
                        .when(this.saveDirtyRemarks(saveOrigin))
                        .always(complete);
                },
                canExecute: (isExecuting) => {
                    var res = !isExecuting && this.isDirty();
                    return res;
                }
            })

            this.onRemarkSelected = (remark: App.Model.Remark) => {/* To be overriden in descendants. */ }

            this.selectRemark = (remark, event = null) => {
                var old = this.selectedRemark();
                this.selectedRemark(remark);
                // A remark can be selected manually or programmatically
                if ((remark !== old) || !event) {
                    this.onRemarkSelected(remark);
                    if (remark) {
                        this.playRemarkSpot(remark);
                    }
                }
                // +http://knockoutjs.com/documentation/click-binding.html#note_3_allowing_the_default_click_action
                return true;
            }

            this.createSound = () => {
                this.sound = window['soundManager'].createSound({
                    id: 'mySound',
                    url: (<any>App).soundUrlParam,
                    autoLoad: true,
                    multiShot: false,

                    onload: (success) => {
                        if (success) {
                            $('#editor').show();
                        }
                        else {
                            toastr.error('Error loading the sound file.');
                        }
                    },

                    whileplaying: () => {
                        (<any>$('#slider')).slider("value", this.sound.position);
                        this.soundPosition(this.sound.position);
                    },

                    onfinish: () => {
                        this.turnPlayer(false);
                    }
                });
            }

            this.formattedPosition = ko.computed(() => {
                return App.Utils.formatMsec(this.soundPosition());
            })

            this.turnPlayer = (play: boolean) => {
                var button = $('#playButton');
                if (play) {
                    if (this.sound.paused) {
                        this.sound.resume();
                    }
                    else {
                        this.sound.play();
                    }
                    button.addClass('active');
                    this.selectRemark(null);
                }
                else {
                    if (this.sound.playState === 1) {
                        this.sound.pause();
                    }
                    button.removeClass('active');
                }
                button.children('i').removeClass('fa-pause fa-play').addClass(play ? 'fa-pause' : 'fa-play');
                button.children('span').text(play ? 'Pause' : 'Play');
            }

            this.playerClick = () => {
                this.turnPlayer(!$('#playButton').hasClass('active'));
            }

            this.playRemarkSpot = (remark) => {
                this.sound.stop();
                this.sound.play({ from: remark.start(), to: remark.finish() });
            }

            this.getRemarkRowClass = (remark) => {/* To be overriden in descendants. */ };

            this.getSpotMarkClass = (remark) => {
                var selectedClass = remark === this.selectedRemark() ? 'selected-mark' : '';
                var bottomClass = remark.reviewId === this.review.id ? '' : ' bottom-mark';
                return selectedClass + bottomClass;
            };
        } // end of ctor

        sortRemarks() {
            this.remarks.sort(
                function (left, right) {
                    var l = left.start() + left.finish(),
                        r = right.start() + right.finish();
                    return l === r ? 0 : (l < r ? -1 : 1);
                }
                );
        }

        // The folowing logic supports multiple possible requests and thus is able to combine remarks from multiple reviews on the same page.
        loadRemarks() {
            var requests = $.map(this.exercise().reviews(), function (i) {
                return App.Utils.ajaxRequest('GET', App.Utils.remarksApiUrl('Review/' + i.id));
            });

            if (requests.length > 0) {
                App.Utils.activityIndicator(true);

                $.when.apply($, requests)
                    .done(() => {
                        var dtoArr = [];

                        if (requests.length > 1) {
                            $.each(arguments, (i, arg) => { dtoArr = dtoArr.concat(arg[0]); });
                        }
                        else {
                            dtoArr = dtoArr.concat(arguments[0]);
                        }

                        $.each(dtoArr, (i, dto) => {
                            var r = new App.Model.Remark(dto);
                            r.dirtyFlag().reset();
                            this.remarks().push(r);
                        });

                        this.sortRemarks();
                    })
                    .fail(function () { toastr.error('Error loading remarks.'); })
                    .always(function () {
                        App.Utils.activityIndicator(false);
                    });
            }
        }

        rewind() {
            var newPosition = Math.max(this.sound.position - 5000, 0);
            this.sound.setPosition(newPosition);
            (<JQueryUI.UI><any>$('#slider')).slider("value", newPosition);
            this.soundPosition(newPosition);

            this.selectRemark(null);
        }

        onSliderChange(event, ui) {
            if (event.originalEvent) {
                this.sound.setPosition(ui.value);
                this.soundPosition(ui.value);
                this.turnPlayer(true);
            }
        }

        onSliderSlide(event, ui) {
            if (event.originalEvent) {
                this.soundPosition(ui.value);
                this.turnPlayer(false);
            }
        }

        setupControls() {
            $('#editor').hide(); // will be shown after the sound is loaded

            (<JQueryUI.UI><any>$('#slider'))
                .slider(
                {
                    min: 0,
                    max: this.exercise().length,
                    change: (event: Event, ui: JQueryUI.SliderUIParams) => this.onSliderChange(event, ui),
                    slide: (event: Event, ui: JQueryUI.SliderUIParams) => this.onSliderSlide(event, ui)
                });
        }

        saveDirtyRemarks(arg) {
            var remarksToSave = ko.utils.arrayFilter(this.remarks(), (i) => { return i.dirtyFlag().isDirty(); });

            // A remark may being edited at the moment of the ajax request. 
            ko.utils.arrayForEach(remarksToSave, (i) => { i.dirtyFlag().reset(); });

            return App.Utils.ajaxRequest(
                'PUT',
                //App.Utils.remarksApiUrl('?source=' + this.savingSourceName),
                App.Utils.remarksApiUrl(this.savingSourceName),
                this.saveMapper(remarksToSave)
                )
                .done(() => {
                    if (typeof arg == typeof Function) {
                        arg(true);
                    }
                    else {
                        if (arg && (typeof arg == 'object') && arg.hasOwnProperty('internalOnSaveDone')) {
                            this.internalOnSaveDone();
                        }
                    }
                    this.dirtyAutoSaveInterval = 0;
                })
                .fail((jqXHR) => {
                    // Possible error causes: 
                    // 1. The review time expired and the review has been finished forcefully on the server. 
                    // 2. The authorization cookie expired and the user has to relogin. 
                    // 3. The user pulled the plug :-)

                    toastr.error('Error saving remarks.');

                    // Error 400 "Bad request" is send by the server.
                    if (jqXHR.status === 400) {
                        toastr.warning('The page will be reloaded in 5 seconds.');
                        window.setTimeout(() => { window.location.reload(true); }, 5000);
                    }
                    else {
                        this.dirtyAutoSaveInterval = this.dirtyAutoSaveInterval === 0 ? 2000 : this.dirtyAutoSaveInterval * 2;
                        // Return remarks to the dirty state on unknown failure to continue saving attempts.
                        ko.utils.arrayForEach(remarksToSave, (i) => {
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
        }

        getSpotMarkStyleLeft(remark) {
            // percent
            return 50 * (remark.start() + remark.finish()) / this.exercise().length;
        }

        setupOnBeforeUnload() {
            window.onbeforeunload = (event) => {
                this.internalOnBeforeUnload(event);
            };
        }


    } // end of class

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

    var soundCreator: JQueryDeferred<any> = $.Deferred();
    //  SoundManager must be initialized before DOM onload. Otherwise browser security may block Flash <-> JS communication after certain JQuery DOM manipulations by a third-party code.
    window['soundManager'].setup({
        url: '/Scripts/sm2/',
        preferFlash: true,
        flashVersion: 9, //default = 8
        useFlashBlock: false,
        debugMode: false, // is needed for choosing the name of the script to load  

        onready: () => { soundCreator.resolve(); },

        ontimeout: () => { toastr.error('Error. Audio player could not start.'); }
    });
}
