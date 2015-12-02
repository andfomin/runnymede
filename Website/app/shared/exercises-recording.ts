module app.exercises {

    export class AudioPlayer {

        exercise: IExercise;
        card: ICard;
        cardItem: ICardItem;
        player: IPlayer;
        remark: IRemark;
        sliderPosition: number = 0;
        sliderWatcher: angular.IPromise<any> = undefined;

        static $inject = [app.ngNames.$filter, app.ngNames.$interval, app.ngNames.$scope, app.ngNames.$timeout,
            app.exercises.CardProvider.ServiceName, app.exercises.PlayerProvider.ServiceName];
        constructor(
            private $filter: angular.IFilterService,
            private $interval: angular.IIntervalService,
            public $scope: angular.IScope,
            private $timeout: angular.ITimeoutService,
            cardProvider: ILazyProvider<ICard>,
            playerProvider: ILazyProvider<IPlayer>
        )
        /* ----- Constructor  ------------ */ {
            (<any>$scope).vma = this; // The inner scope. The outer scope is "vm".

            this.exercise = app['exerciseParam'];

            $scope.$on('$destroy', () => { this.stopSliderWatcher(); });

            cardProvider.get()
                .then((card) => {
                    this.card = card;
                    // Attribute parts of the recording to questions.
                    var details = angular.fromJson(this.exercise.details);
                    var trackDurations: { [trackId: string]: number } = (details && details.trackDurations) || {};
                    this.card.items.reduce((previous: number, current: ICardItem) => {
                        var duration = trackDurations[current.position];
                        current.playFrom = duration ? previous : null;
                        current.playTo = duration ? previous + duration : null;
                        return previous + (duration || 0);
                    }, 0);
                });

            // Download the recording lazily.
            playerProvider.get()
                .then((player) => {
                    this.player = player;
                    $scope.$broadcast('refreshSlider'); // The slider was initially hidden. The doc advises to refresh it on showing. It updates DOM in a $timeout.
                },
                (reason) => { toastr.error(reason.message); }
                );
        }
        /* ----- End of constructor  ----- */

        play = (options?: { from?: number, to?: number }) => {
            this.player.play(options)
                .then(
                () => {
                    if (Math.abs(this.exercise.length - this.player.currentTime) < 0.5) {
                        this.player.currentTime = 0;
                    }
                    this.updateCurrentTime();
                },
                null,
                () => { this.updateCurrentTime(); }
                );
        };

        updateCurrentTime = () => {
            if (!this.sliderWatcher) {
                // The notification interval is usually 250 msec, but may vary. It depends on the performance of the device.
                // Our granularity is 1 sec to limit the frequency of the page repainting. 
                var time = this.player.currentTime;
                if ((Math.abs(Math.round(time) - time) < 0.3) || (Math.abs(time - this.sliderPosition) > 1)) {
                    this.$scope.$evalAsync(() => {
                        this.sliderPosition = time;
                        // Display the corresponding question
                        this.cardItem = app.arrFind(this.card.items, (i) => { return (i.playFrom < time) && (i.playTo > time); });
                    });
                }
            }
        };

        turnPlayer = () => {
            if (this.player.isPlaying()) {
                this.player.stop();
            }
            else {
                this.selectRemark(null);
                this.play();
            }
        }

        rewind = () => {
            this.selectRemark(null);
            this.player.currentTime = Math.max(this.player.currentTime - 5, 0);
            this.updateCurrentTime();
        }

        playRemarkSpot = (remark) => {
            this.player.stop();
            this.play({
                from: remark.start,
                to: remark.finish
            });
        }

        onSliderChange = () => {
            if (this.player.isPlaying()) {
                this.player.stop();
            }
            // We want to start the player as soon as the user releases the pointer. The 'active' class is removed from the pointer element after the last ngChange. We establish a timer to watch for the class removal. 
            if (!this.sliderWatcher) {
                var sliderPointer = $('#slider-container > slider > span.pointer.low');
                // We pass invokeApply = false, so we do not $digest on every tick, only on the last one.
                this.sliderWatcher = this.$interval(() => {
                    if (!sliderPointer.hasClass('active') && this.sliderWatcher) {
                        this.stopSliderWatcher();
                        this.player.currentTime = this.sliderPosition;
                        //this.play({ from: this.sliderPosition });
                    }
                }, 500, 0, false);
            }
        }

        stopSliderWatcher = () => {
            this.$interval.cancel(this.sliderWatcher);
            this.sliderWatcher = null;
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
            return this.$filter<(value) => string>('appSecToMinSec')(value);
        }

        isEditing = (remark: app.IRemark) => {
            var unfinished = this.exercise.reviews.some((i) => { return !i.finishTime; });
            return (remark === this.remark) && unfinished;
        };

    } // end of class AudioPlayer
           
    export function RecordingsComparer(a: IRemark, b: IRemark) {
        var aa = a.start + a.finish;
        var bb = b.start + b.finish;
        return aa === bb ? 0 : (aa < bb ? -1 : 1);
    };

    // Angular uses the suffix as a naming convention to configure the povider.
    artifactPlayerProviderConfig.$inject = [app.exercises.PlayerProvider.ServiceName + 'Provider'];
    export function artifactPlayerProviderConfig(provider: app.IServiceProvider<string>) {
        var exercise: IExercise = app['exerciseParam'];
        var url = app.getBlobUrl('artifacts', exercise.artifact);
        provider.configure(url);
    };


} // end of module app.exercises
