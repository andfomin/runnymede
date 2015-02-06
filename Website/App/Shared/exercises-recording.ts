module app.exercises {

    export class AudioPlayer {

        exercise: IExercise;
        remark: IRemark;
        sound: any = null;
        soundPosition: number = 0;
        autoLoad: boolean = false;
        soundLoaded: boolean = false;
        sliderTimer: ng.IPromise<any> = undefined;
        playing: boolean = false;

        constructor(
            public $scope: app.IScopeWithViewModel,
            private $filter: ng.IFilterService,
            private $interval: ng.IIntervalService,
            private $timeout: ng.ITimeoutService
            )
        /* ----- Constructor  ------------ */
        {
            (<any>$scope).vma = this; // Inner scope. The outer is vm.

            this.exercise = app['exerciseParam'];
            //this.setupSoundManager(); // SoundManager2 does not work if initialized late during the AngularJS bootstraping. It is initilized below as a regular JS script.
            //soundManagerReady.done(() => { this.createSound(); });
            soundManagerReady.promise.then(() => { this.createSound(); });
            $scope.$on('$destroy', () => { this.stopSliderWatcher(); });
        }
        /* ----- End of constructor  ----- */

        createSound = () => {
            var sm = window['soundManager'];
            var versionFlash = sm && sm.version && (sm.version.indexOf('Flash') > 0);
            var usingFlash = versionFlash && sm.html5 && sm.html5.usingFlash;
            this.autoLoad = usingFlash || !app.isMobileDevice();
            this.$scope.$apply();
            //if (app.isDevHost()) {
            //    alert(versionFlash + ';' + usingFlash + ';' + app.isMobileDevice() + ';' + this.autoLoad);
            //};

            var self = this;

            this.sound = sm.createSound({
                id: 'mySound',
                url: app.getBlobUrl('recordings', this.exercise.artifact),
                autoLoad: this.autoLoad, // Should be explicitly false for iOS. Mobile browsers permit downloading only on a user action.
                multiShot: false,

                onload(success) {
                    if (success) {
                        self.$scope.$evalAsync(() => {
                            self.soundLoaded = true; // Show the editor controls.
                            self.$scope.$broadcast('refreshSlider'); // The slider was initially hidden. The doc advises to refresh it on showing. It updates DOM in $timeout.
                        });
                    }
                    else {
                        // False value should seemingly only be for failure, but appears to be returned by Flash for load from cache as well.
                        if (this.readyState != 3) {
                            toastr.error('Error occured while loading the sound file.');
                        }
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
        };

        setPlaying = (playing: boolean) => {
            this.$scope.$evalAsync(() => {
                this.playing = playing;
            });
        };

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
                // If autoLoad is initially false and soundLoaded is false, show the spinner.
                this.autoLoad = true;
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
            if (remark !== this.remark) {
                this.remark = remark;
                if (remark) {
                    this.playRemarkSpot(remark);
                }
            }
        };

        isSelected = (remark) => {
            return (remark === this.remark);
        }

        getSpotMarkPosition = (remark) => {
            return 100 * (remark.start + remark.finish) / 2 / this.exercise.length; // percent
        }

        positionFormatting = (value) => {
            return this.$filter('appMsecToMinSec')(value);
        }

        isEditing = (remark: app.IRemark) => {
            var unfinished = this.exercise.reviews.some((i) => { return !i.finishTime; });
            return (remark === this.remark) && unfinished;
        };

    } // end of class AudioPlayer

    var injector = angular.injector(['ng']);
    var $q: ng.IQService = injector.get('$q');
    var soundManagerReady = $q.defer();

    var setupSoundManager = () => {
        var sm = window['soundManager'];
        if (sm) {
            sm.setup({
                url: '/scripts/sm2/',
                flashVersion: 9, //default = 8
                preferFlash: true, // HTML5 Audio play/pause/resume is fragile in Chrome and in iOS Safari.
                useFlashBlock: false,
                debugMode: false, // Set it explicitly, it is needed for choosing the name of the script to load.
                useConsole: false,
                waitForWindowLoad: true,
                onready: () => { soundManagerReady.resolve(); },
                ontimeout: () => { toastr.error('Error. Audio player could not start.'); }
            });
        }
        else {
            window.setTimeout(setupSoundManager, 500);
        }
    };

    //  SoundManager must be initialized before the DOM onload event. Otherwise browser security may block Flash <-> JS communication after certain JQuery DOM manipulations by a third-party code.
    // If setupSoundManager() is called within the controller constructor, SM2 ignores the Flash setting and chooses the HTML5 mode (which does not work in Chrome properly.)
    setupSoundManager();

    export function RecordingsComparer(a: IRemark, b: IRemark) {
        var aa = a.start + a.finish;
        var bb = b.start + b.finish;
        return aa === bb ? 0 : (aa < bb ? -1 : 1);
    };

} // end of module app.exercises
