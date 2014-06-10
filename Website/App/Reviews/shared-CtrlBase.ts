module App.Reviews {

    // The code was initially written for JQuery + KnockoutJS, then ported to AngularJS. That's why it looks not Angular-way. 
    export class CtrlBase {

        sound: any = null;
        exercise: App.Model.IExercise2;
        reviews: App.Model.IReview2[];
        remarks: App.Model.IRemark2[] = [];
        selectedRemark: App.Model.IRemark2 = null;
        soundPosition: number = 0;
        soundLoaded: boolean = false;
        sliderTimer: ng.IPromise<any> = undefined;
        playing: boolean = false;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http, App.Utils.ngNames.$filter, App.Utils.ngNames.$interval,
            App.Utils.ngNames.$modal, App.Utils.ngNames.$q];

        constructor(
            public $scope: Utils.IScopeWithViewModel,
            public $http: ng.IHttpService,
            public $filter: ng.IFilterService,
            public $interval: ng.IIntervalService,
            public $modal: ng.ui.bootstrap.IModalService,
            public $q: ng.IQService
            ) {
            /* ----- Ctor  ----- */
            this.exercise = (<any>App).exerciseParam;
            this.reviews = this.exercise.reviews;

            this.reviews.forEach((r) => {
                r.suggestions.forEach((s) => {
                    s.dirtyTime = null;
                });
            });

            this.loadRemarks();
            soundCreator.done(() => { this.createSound(); });
            $scope.$on('$destroy', () => { this.stopSliderWatcher(); });
        } // end of ctor

        //loadRemarks0 = () => {
        //    // We want to combine remarks from multiple reviews. Make multiple simultaneous requests.
        //    var requests = this.exercise.reviews.map((i) => {
        //        return App.Utils.ngHttpGetNoCache(this.$http,
        //            App.Utils.remarksApiUrl('Review/' + i.id),
        //            null,
        //            () => { }
        //            );
        //    });

        //    if (requests.length > 0) {
        //        // Combine multiple promises into a single promise that is resolved when all of the input promises are resolved.
        //        this.$q.all(requests)
        //            .then(
        //            (responses: any[]) => {
        //                this.remarks = [];
        //                responses.forEach((i) => {
        //                    var data: App.Model.IRemark2[] = i.data;
        //                    data.forEach((r) => {
        //                        r.dirtyTime = null;
        //                        this.remarks.push(r);
        //                    });
        //                });
        //                this.sortRemarks();
        //            },
        //            () => { toastr.error('Error loading remarks.'); }
        //            );
        //    }
        //}

        loadRemarks = () => {
            var reviewCount = this.exercise.reviews.length;
            if (reviewCount > 0) {
                var route = reviewCount === 1 ? 'Review/' + this.reviews[0].id : 'Exercise/' + this.exercise.id;

                App.Utils.ngHttpGetNoCache(this.$http,
                    App.Utils.remarksApiUrl(route),
                    null,
                    (data) => {
                        this.remarks = data;
                        this.sortRemarks();
                        this.remarks.forEach((i) => {
                            i.dirtyTime = null;
                        });
                    }
                    );
            }
        }

        createSound = () => {
            this.sound = window['soundManager'].createSound({
                id: 'mySound',
                url: (<any>App).soundUrlParam,
                autoLoad: true, // Mobile browsers permit downloading only on a user action.
                multiShot: false,

                onload: (success) => {
                    if (success) {
                        this.$scope.$evalAsync(() => {
                            this.soundLoaded = true; // Show the editor controls.
                            this.$scope.$broadcast('refreshSlider'); // The slider was initially hidden. The doc advises to refresh it on showing. It updates DOM in $timeout.
                        });
                    }
                    else {
                        toastr.error('Error loading the sound file.');
                    }
                },

                whileplaying: () => {
                    if (!angular.isDefined(this.sliderTimer)) {
                        this.$scope.$evalAsync(() => {
                            this.soundPosition = this.sound.position;
                        });
                    }
                },

                onplay: () => {
                    this.setPlaying(true);
                },

                onresume: () => {
                    this.setPlaying(true);
                },

                onstop: () => {
                    this.setPlaying(false);
                },

                onpause: () => {
                    this.setPlaying(false);
                },

                onfinish: () => {
                    this.setPlaying(false);
                }
            });
        }

        setPlaying = (playing: boolean) => {
            this.$scope.$evalAsync(() => {
                this.playing = playing;
            });
        }

        turnPlayer = (play: boolean) => {
            if (play) {
                if (this.sound.paused) {
                    this.sound.resume();
                }
                else {
                    if (Math.abs(this.sound.duration - this.sound.position) < 100) {
                        this.sound.setPosition(0);
                    }
                    this.sound.play();
                }
                this.selectRemark(null);
            }
            else {
                if (this.sound.playState === 1) {
                    this.sound.pause();
                }
            }
        }

        rewind = () => {
            var newPosition = Math.max(this.sound.position - 5000, 0);
            this.soundPosition = newPosition;
            this.sound.setPosition(newPosition);
            this.selectRemark(null);
        }

        playRemarkSpot = (remark) => {
            this.sound.stop();
            this.sound.play({
                from: remark.start,
                to: remark.finish
            });
        }

        onSliderChange = () => {
            this.turnPlayer(false);
            // We want to start the player as soon as the user releases the pointer. The 'active' class is removed from the pointer element after the last ngChange. We establish a timer to watch for the class removal. 
            if (!angular.isDefined(this.sliderTimer)) {
                var sliderPointer = $('#slider-container > slider > span.pointer.low');
                // We pass invokeApply = false, so we do not $digest on every tick, only on the last one.
                this.sliderTimer = this.$interval(() => {
                    if (!sliderPointer.hasClass('active') && angular.isDefined(this.sliderTimer)) {
                        this.sound.setPosition(this.soundPosition);
                        this.turnPlayer(true);
                        this.stopSliderWatcher(); // $interval.cancel calls $digest
                    }
                }, 100, 0, false);
            }
        }

        stopSliderWatcher = () => {
            if (angular.isDefined(this.sliderTimer)) {
                this.$interval.cancel(this.sliderTimer);
                this.sliderTimer = undefined;
            }
        }

        selectRemark = (remark) => {
            if (remark !== this.selectedRemark) {
                this.selectedRemark = remark;
                if (remark) {
                    this.playRemarkSpot(remark);
                }
            }
        }

        isSelected = (remark) => {
            return (remark === this.selectedRemark);
        }

        getSpotMarkPosition = (remark) => {
            return 100 * (remark.start + remark.finish) / 2 / this.exercise.length; // percent
        }

        sortRemarks = () => {
            this.remarks.sort(
                function (left, right) {
                    var l = left.start + left.finish;
                    var r = right.start + right.finish;
                    return l === r ? 0 : (l < r ? -1 : 1);
                });
        }

        positionFormatting = (value) => {
            return this.$filter('appMsecToMinSec')(value);
        }

    } // end of class

    var soundCreator: JQueryDeferred<any> = $.Deferred();

    var setupSoundManager = () => {
        var soundManager = window['soundManager'];
        if (soundManager) {
            //  SoundManager must be initialized before the DOM onload event. Otherwise browser security may block Flash <-> JS communication after certain JQuery DOM manipulations by a third-party code.
            soundManager.setup({
                url: '/Scripts/sm2/',
                preferFlash: true, // HTML5 Audio play/pause/resume works wrong.
                flashVersion: 9, //default = 8
                useFlashBlock: false,
                debugMode: false, // is needed for choosing the name of the script to load  

                onready: () => { soundCreator.resolve(); },

                ontimeout: () => { toastr.error('Error. Audio player could not start.'); }
            });
        }
        else {
            window.setTimeout(setupSoundManager, 500);
        }
    };
    setupSoundManager();

} // end of module

var app = angular.module('app', ['AppUtilsNg', 'ui.bootstrap', 'chieffancypants.loadingBar', 'vr.directives.slider']);
