module app.exercises {

    export interface IPlayer {
        play: (options?: { from?: number, to?: number }) => angular.IPromise<any>; // seconds 
        stop: () => void;
        currentTime: number; // getter setter
        //duration: number; // Safari iOS gives a shorter duration, updates it only after the recording is played up to the end.
        isPlaying: () => boolean;
    }

    interface IPlayerTrack extends angular.IDeferred<any> {
        to: number; // seconds
        endTime: number; // milliseconds, Date.now()
        interval: angular.IPromise<any>;
    }

    class Player implements IPlayer {

        private track: IPlayerTrack;

        constructor(
            private audio: HTMLAudioElement,
            private $interval: angular.IIntervalService,
            private $q: angular.IQService
        ) {
            this.audio.addEventListener('timeupdate', this.onTimeUpdate);
            this.audio.addEventListener('ended', this.stop);
        };

        onTimeUpdate = () => {
            /* Chrome Windows fires 'ontimeupdate' every 250 msec. Safari iOS ~250 msec. 
            The spec allows the browser to measure the event handler duration and adjust the event frequency accordingly to avoid performance impact.
            */
            var currentTime = this.audio.currentTime;
            if (this.track) {
                if (this.track.to && (currentTime > this.track.to)) {
                    this.stop();
                }
                this.track.notify(currentTime);
            }
        };        

        /* The first call of play() in iOS and Android must be on a user gesture.
        My observation is that the first play() call in Safari iOS does not have to be exactly on a user event call stack.
        It may be in around one second proximity. So a call on a timer with a delay 500 msec may work, 
        whereas with a delay 2000 msec may not. It needs thorough testing though.
        */
        play = (options?: { from?: number, to?: number }) => {
            this.stop();

            this.track = <IPlayerTrack>this.$q.defer();

            if (options && !app.isUndefinedOrNull(options.from)) {
                this.audio.currentTime = options.from;

                if (!app.isUndefinedOrNull(options.to)) {
                    this.track.to = options.to;

                    /* To watch for the end we listen to 'timeupdate' and also setup a timer which does not depend on external events.
                      Safari iOS freezes timers when the page loses focus. Thus we use $interval for the watchdog, not $timeout.
                      Watching audio.currentTime in 'ontimeupdate' has higher priority for us. Our stopwatch is simply an auxilary guard.
                    */
                    var duration = Math.max((options.to - options.from), 0) * 1000; // seconds -> milliseconds
                    var delay = 500;
                    var count = Math.ceil(duration / delay);
                    this.track.interval = this.$interval(() => {
                        if (this.track && this.track.endTime && (Date.now() > this.track.endTime)) {
                            this.stop();
                        }
                    }, delay, count, false);
                    // Do not chain the then() to the promise directly, store the reference in a variable. The interval object holds $intervalId internally wich will be lost in a chained promise.
                    this.track.interval.then(() => {
                        this.stop();
                    });

                    this.track.endTime = Date.now() + duration;
                }
            }

            this.audio.play();
            return this.track.promise;
        };

        stop = () => {
            if (!this.audio.paused) {
                this.audio.pause();
            }
            if (this.track) {
                this.$interval.cancel(this.track.interval);
                this.track.resolve();
                this.track = null;
            }
        };

        public get currentTime(): number {
            return this.audio.currentTime;
        }

        public set currentTime(value: number) {
            this.audio.currentTime = value;
        }

        isPlaying = () => {
            return !!this.track;
        };

    } // end of class Player

    export class PlayerProvider implements angular.IServiceProvider, app.IServiceProvider<string> {
        static ServiceName = 'appPlayerProvider';

        url: string;
        promise: angular.IPromise<IPlayer>;

        configure = (param: string) => {
            this.url = param;
        };

        $get = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$q,
            (
                $http: angular.IHttpService,
                $interval: angular.IIntervalService,
                $q: angular.IQService
            ) => {
                return <ILazyProvider<IPlayer>>{
                    get: () => {
                        if (!this.promise) {
                            var deferred = $q.defer<IPlayer>();
                            this.promise = deferred.promise;
                            /* We do not use Web Audio API for the Speaker service because of the Safari iOS limitations.
                            When the user launches the video recording app, the OS hijacks/switches the audio hardware from the web page. (It may be because iOS prior to ver 9 is a single-task OS.)
                            Although our JavaScript AudioContext object looks the same on return, it is not actually connected anymore to the hardware.
                            We cannot create new instances of AudioContext on the fly because of the limit of 4 AudioContexts per window (Error "audio resources unavailable for AudioContext construction")
                            */
                            /* The HTMLAudioElement does not start loading the file until it started playing. It can cause delays.
                            We pre-download the file manually and create a Blob which is assigned to the HTMLAudioElement, so the file is immidatelly
                            available locally.
                            */
                            $http.get<Blob>(this.url, { responseType: "blob", })
                                .then(
                                (response) => {
                                    var audio = new Audio();
                                    audio.onloadedmetadata = (arg) => {
                                        var speaker = new Player(audio, $interval, $q);
                                        deferred.resolve(speaker);
                                    };
                                    audio.src = URL.createObjectURL(response.data);
                                },
                                (response) => { deferred.reject({ message: response.statusText, }); }
                                );
                        }
                        return this.promise;
                    },
                };
            }
        ];

    } // end of class PlayerProvider

} // module app.exercises 