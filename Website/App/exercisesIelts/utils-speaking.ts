module app.exercisesIelts {

    export interface IRecorder {
        createRecorder: () => angular.IPromise<IRecorder>;
        startRecorder: () => angular.IPromise<any>;
        stopRecorder: () => void;
        startTrack: (trackId: string) => void;
        stopTrack: (data?: ArrayBuffer) => angular.IPromise<any>;
        tracks: { [trackId: string]: Blob };
    }

    export class SpeakingBase {

        card: ICard;
        player: app.exercises.IPlayer;
        recorder: IRecorder;
        item: ICardItem; // undefined means not started, null means stopped.
        cueCard: string;
        started: Date;
        isRepeating: boolean;
        playbacks: { [trackId: string]: HTMLAudioElement } = {};
        title: string;
        stopTimer: angular.IPromise<any>;

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$rootScope, app.ngNames.$scope, ngNames.$state,
            app.ngNames.$window];
        constructor(
            private $http: angular.IHttpService,
            private $interval: angular.IIntervalService,
            $rootScope: angular.IRootScopeService,
            private $scope: app.IScopeWithViewModel,
            private $state: angular.ui.IStateService,
            protected $window: angular.IWindowService
        ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;

            $rootScope.$on('$stateChangeSuccess',
                (event, toState: angular.ui.IState, toParams, fromState: angular.ui.IState, fromParams) => {
                    $scope.$evalAsync(() => { this.onStateChanged(toState, fromState); });
                }
            );
            this.go(States.Ready);
            /* ----- End of constructor  ----- */
        } // end of ctor 

        // The fromState parameter might be used to prevent the transition if the current state has changed before the associated promise is resolved. 
        // We may have got to Stopped while there is still some promise waiting in the prevoious state. 
        go = (toState: string, fromState?: string) => {
            if (angular.isUndefined(fromState) || this.$state.is(fromState)) {
                this.$state.go(toState);
            }
        };

        isState = (...states: string[]) => {
            //return states.some((i) => this.$state.inludes(i));
            return states.indexOf(this.$state.current.name) != -1;
        };

        onStateChanged = (toState: angular.ui.IState, fromState: angular.ui.IState) => {
            //console.log(moment.utc().toISOString(), fromState.name, ' >>> ', toState.name);

            switch (toState.name) {
                case States.Ready:
                    // Promises declared in the state are resolved/rejected at this point. Notice that $state.$current !== $state.current
                    var resolved = (<any>this.$state).$current.locals.globals;
                    this.card = resolved.card;
                    this.player = resolved.player;
                    this.recorder = resolved.recorder;
                    if (!this.card || !this.player || !this.recorder) {
                        this.go(States.Error);
                    }
                    // Reduce the card while developing and testing
                    if (app.isDevHost()) {
                        this.card.items.splice(17, 5);
                        this.card.items.splice(2, 8);
                    }
                    break;
                case States.Starting:
                    // The first play in mobile browsers must be on a call stack within a user click. (Probably may occur within one second from the click)
                    this.player.play({
                        from: 0,
                        to: 0,
                    })
                        .then(() => {
                            this.recorder.startRecorder()
                                .then(
                                () => {
                                    this.started = new Date();
                                    // Force stop on timeout even without any further user action. We assume the last card item has a paceTarget value.
                                    var timeout = this.card.items.reduce((previous: number, current: ICardItem) => {
                                        var paceTarget = app.isDevHost() ? current.paceTarget2 : current.paceTarget;
                                        return (paceTarget > previous) ? paceTarget : previous;
                                    }, 0);
                                    this.stopTimer = this.setTimeout(timeout);
                                    /* Apparently, as getUserMedia() starts capturing, the JS engine is getting very busy here (JIT-compiles new functions?).
                                     For example, HowlerJS uses Web Audio API, which apparently has low priority and simply stops playing audio.
                                     If a track is started immediately by the player's Audio, it starts playing silently, than the sound is unmuted starting from 3 sec (the currentTime advances).
                                     We might need wait a little for the JS engine to become idle.
                                     The delay value 1 second is empirical, tested on the dual-core 1GHz Android phone; 500 msec trims the beginning of the track.
                                    */
                                    this.$interval(() => { this.go(States.Idle); }, 1000, 1, false);
                                },
                                (reason) => { toastr.error(reason.message); }
                                );
                        });
                    break;
                case States.Idle:
                    this.player.stop();
                    // If there is no track being recorded, we will simply receive a blank rejected promise.
                    this.recorder.stopTrack()
                        .finally(() => {
                            this.forward();
                        });
                    break;
                case States.Playing:
                    var to = (this.item.record === false) ? States.Idle : States.Recording;
                    // Be carefull. The first play in mobile browsers must be on a call stack within a user click. (Probably may occur within one second from the click)
                    this.player.play({
                        from: this.item.playFrom,
                        to: app.isDevHost() ? (this.item.playTo2 || this.item.playTo) : this.item.playTo,
                    })
                        .then(() => { this.go(to, States.Playing); });
                    break;
                case States.Repeating:
                    this.recorder.stopTrack()
                        .finally(() => {
                            this.player.play({
                                from: this.item.playFrom,
                                to: app.isDevHost() ? (this.item.playTo2 || this.item.playTo) : this.item.playTo,
                            })
                                .then(() => { this.go(States.Recording, States.Repeating); });
                        });
                    break;
                case States.Recording:
                    if (this.item.timeout) {
                        this.setTimeout(this.item.timeout, this.item);
                    }
                    this.recorder.startTrack(this.item.position);
                    break;
                case States.Pause:
                    this.setTimeout(this.item.timeout, this.item);
                    break;
                case States.Stopped:
                    this.$interval.cancel(this.stopTimer);
                    this.player.stop();
                    this.recorder.stopRecorder();
                    this.item = null;
                    this.cueCard = null;
                    this.copyCueCard();
                    this.createPlaybacks();
                    break;
                case States.Uploading:
                    this.upload();
                    break;
                case States.Uploaded:
                    break;
                case States.Error:
                    break;
                default:
                    break;
            }
        };

        setTimeout = (timeoutSec: number, item?: ICardItem) => {
            var timeoutMsec = timeoutSec * 1000;
            var delay = 1000;
            var count = Math.ceil(timeoutMsec / delay) + 1;
            var end = new Date().getTime() + timeoutMsec;
            // Do not use angular.ITimeoutService because mobile browsers may suspend the timer on app/tab switch.
            var interval = this.$interval(() => {
                if (new Date().getTime() > end) {
                    this.$interval.cancel(interval);
                    if ((this.item === item) || angular.isUndefined(item)) {
                        this.go(States.Idle);
                    }
                }
            }, delay, count, false);
            return interval;
        };

        forward = () => {
            var index = this.card.items.indexOf(this.item);
            if ((index < this.card.items.length - 1) && (this.item !== null)) {
                index++;                
                // If it's time to move to the next part, jump ahead.
                var pace = (new Date().getTime() - this.started.getTime()) / 1000; // seconds
                this.item = this.card.items.reduce(
                    (previous: ICardItem, current: ICardItem, idx: number) => {
                        var paceTarget = app.isDevHost() ? current.paceTarget2 : current.paceTarget;
                        return ((idx > index) && (paceTarget <= pace))
                            ? current
                            : previous;
                    },
                    this.card.items[index]
                );

                this.isRepeating = false;
                this.executeItem();
            }
            else {
                this.go(States.Stopped);
            }
        }

        executeItem = () => {
            if (!angular.isUndefined(this.item.cueCard)) {
                this.cueCard = this.item.cueCard;
            }
            if (!app.isUndefinedOrNull(this.item.playFrom)) {
                this.go(States.Playing);
            }
            else
                if (!app.isUndefinedOrNull(this.item.timeout)) {
                    this.go(States.Pause);
                }
        };

        createPlaybacks = () => {
            /* The Audio element in Android Chrome does not load locally created blob URLs. 
            Apparently it cannot read some metadata from the blob (tested with MP3). 
            The Audio element can read blobs created by XMLHttpRequest though, which in turn accepts the "blob:" schema.
            We use an XMLHttpRequest as a proxy to create a blob URL.
            Here is another sad story about Android Chrome. Based on observations if we call play() the first time after longer than 1 second from the user interaction, it ignores it. 
            We cannot be sure that loading the blob on a slow device will take less than 1 sec. 
            Moreover, there are rumors that Audio in mobile browsers have hard time with changing the src attribute on the fly. It has data buffer inertia. (it can start playing the first source, but not play another one until it is loaded).
            So we preload the audio tracks in separate Ausio elements and play each on a user click.           
             */
            // We do not offer the user to save the recording locally. IE and iOS Safari do not support the "download" attribute in the Anchor element.
            var tracksIds = this.getTracksIds();
            tracksIds.forEach((i) => {
                var blob = this.recorder.tracks[i];
                var blobUrl = URL.createObjectURL(blob);
                // The Angular's $http triggers digest cicle on response. We use the raw XHR to avoid digests.
                var xhr = new XMLHttpRequest();
                xhr.open("GET", blobUrl);
                xhr.responseType = "blob"; // In IE, .open() must precede setting .responseType.
                xhr.onload = () => {
                    var src = URL.createObjectURL(xhr.response);
                    var audio = new Audio(src);
                    this.playbacks[i] = audio;
                    // Run digest after all tracks are loaded.
                    if (Object.keys(this.playbacks).length === tracksIds.length) {
                        this.$scope.$applyAsync();
                    }
                };
                xhr.send();
            }
            );
        };

        stopPlayback = () => {
            this.player.stop();
            this.getTracksIds()
                .forEach((i) => {
                    var audio = this.playbacks[i];
                    if (!audio.paused) {
                        audio.pause();
                    }
                });
        };

        getTracksIds = () => {
            // We must keep the file order. angular.forEach() internally uses "for (key in obj)" which iterates in arbitrary order.
            return Object.keys(this.recorder.tracks).sort();

        };

        getPlaybackItems = () => {
            //var cue = app.arrFind(this.card.items, (i) => { return !!i.cueCard; });
            return !!this.card &&
                this.card.items.filter((i) => {
                    return !!this.recorder.tracks[i.position];
                });
        };

        copyCueCard = () => {
            var fromItem = app.arrFind(this.card.items, (i) => { return !!i.cueCard; });
            var toItem = app.arrFind(this.card.items, (i) => { return (i.position > fromItem.position) && (i.record !== false); });
            toItem.content = fromItem.cueCard;
        };

        upload = () => {
            var data = new FormData();

            var metadata = {
                serviceType: app['serviceTypeParam'],
                cardId: this.card.id,
                title: this.title,
                comment: null,
            };
            const metadataName = 'metadata' + Date.now();
            data.append(metadataName, angular.toJson(metadata)); // String is accepted in the two-parameter version only.

            this.getTracksIds()
                .forEach((i) => {
                    data.append(i, this.recorder.tracks[i]);
                });

            this.$http.post(app.exercisesApiUrl('save_tracks?metadataName=' + metadataName),
                data,
                {
                    transformRequest: angular.identity, // Leave the FormData intact                    
                    headers: { 'Content-Type': undefined } // The browser will set the Content-Type to 'multipart/form-data' and fill in the correct multi-part boundary.
                })
                .then(
                (response) => {
                    var data: any = response.data;
                    if (data && data.exerciseId) {
                        this.$window.location.assign('/exercises/view/' + data.exerciseId);
                    }
                    else
                        if (data && data.key) {
                            this.$window.location.assign('/account/signup?returnUrl=%2Fexercises%2Fclaim%2F' + data.key);
                        }
                        else {
                            this.go(States.Uploaded);
                        }
                },
                app.logError
                );
        };

        // Control handlers  ---------------------------------------------------------------

        start = () => {
            this.go(States.Starting);
        };

        // The "Next question" button
        next = () => {
            this.go(States.Idle);
        };

        canRepeat = () => {
            return this.isState(States.Recording) && !app.isUndefinedOrNull(this.item.playFrom);
        };

        repeat = () => {
            this.isRepeating = true;
            this.go(States.Repeating);
        };

        canStop = () => {
            return this.isState(States.Idle, States.Playing, States.Repeating, States.Recording, States.Pause);
        };

        stop = () => {
            this.go(States.Stopped);
        };

        canSave = () => {
            return this.isState(States.Stopped);
        };

        save = () => {
            this.go(States.Uploading);
        };

        canPlayBack = (item: ICardItem) => {
            return this.playbacks.hasOwnProperty(item.position);
        }

        playBack = (item: ICardItem) => {
            this.stopPlayback();
            var audio = this.playbacks[item.position];
            audio.currentTime = 0;
            audio.play();
        };

    } // end of class SpeakingBase

    // A utillity class used for debugging the code of a Worker in the main thread. It mocks the Worker environment.
    //class WorkerProxy implements Worker {

    //    private eventTarget: DocumentFragment; // DocumentFragment extends Node. Node extends EventTarget
    //    myOnmessage: (e: {
    //        data: {
    //            command: string;
    //            buffers?: any[];
    //        }
    //    }) => void;

    //    callback: (message: any, ports?: any) => void;

    //    constructor() {
    //        this.eventTarget = document.createDocumentFragment();
    //        this.myOnmessage = (<any>window).my_onmessage;
    //        (<any>window).my_proxy = this.my_proxy;
    //    }

    //    addEventListener = (type: string, listener: EventListenerOrEventListenerObject, useCapture?: boolean) => {
    //        this.eventTarget.addEventListener(type, listener, useCapture);
    //    };

    //    // The following methods are not used. We implement the EventTarget interface.
    //    removeEventListener = (type, listener, useCapture) => {
    //        this.eventTarget.removeEventListener(type, listener, useCapture);
    //    };

    //    dispatchEvent = (evt: Event) => {
    //        return this.eventTarget.dispatchEvent(evt);
    //    };

    //    onmessage = (ev: MessageEvent) => { };

    //    postMessage = (message: any, ports?: any) => {
    //        var e = {
    //            data: message,
    //        };
    //        this.myOnmessage(e);
    //    };

    //    terminate = () => { };
    //    onerror = (ev: Event) => { };

    //    my_proxy = {
    //        postMessage: (message: any, ports?: any) => {
    //            // console.log(message);
    //            this.callback(message, ports);
    //            //var event: MessageEvent = document.createEvent('MessageEvent');
    //            //event.initMessageEvent('message', false, false, message, '', '', null);
    //            //this.dispatchEvent(event);                                
    //        }
    //    };

    //    setCallback = (callback: (message: any, ports?: any) => void) => {
    //        this.callback = callback;
    //    }
    //}
    
    export class EncoderFabric implements ILazyProvider<Worker> {
        static ServiceName = 'appEncoderService';

        private promise: angular.IPromise<Worker>; // singleton;

        static $inject = [app.ngNames.$q];
        constructor(
            private $q: angular.IQService
        ) {
        };

        get = () => {
            if (!this.promise) {
                var deferred = this.$q.defer<Worker>();
                this.promise = deferred.promise;
                var path = app['encoderPath'];
                if (path) {
                    // lame.js is imported in utils-encoder.js. The scripts utils-encoder.js and lame.js are lazy-loaded.
                    var encoder = new Worker(path);
                    encoder.addEventListener('error', (ev: ErrorEvent) => { console.log(ev); });
                    var eventListener = (ev: MessageEvent) => {
                        deferred.resolve(encoder);
                        encoder.removeEventListener('message', eventListener);
                    };
                    encoder.addEventListener('message', eventListener);
                    encoder.postMessage({ command: 'ping', }); // ping the worker and wait until it responds.
                }
                else {
                    deferred.reject({ message: 'encoderPath not found' });
                }
            }
            return this.promise;
        };

    } // end of class EncoderFabric

    interface IRecorderTrack extends angular.IDeferred<any> {
        trackId: string;
        isRecording: boolean;
    }

    export abstract class RecorderBase implements IRecorder {

        encoder: Worker;
        deferredCreate: angular.IDeferred<any>;
        internalCreateRecorder: () => void;
        internalStartTrack: () => void;
        internalStopTrack: (data?: ArrayBuffer) => void;
        encoderEventListener: (ev: MessageEvent) => void;
        track: IRecorderTrack;
        unstartedTrack: IRecorderTrack;
        tracks: { [trackId: string]: Blob } = {};

        // We don't declare $inject here because this object is created by a factory which has got the services injected and passes them directly to the ctor.
        // Ctor parameters are determined by RecorderDesktop.
        constructor(
            public $q: angular.IQService,
            public $timeout: angular.ITimeoutService,
            public $window: angular.IWindowService,
            public encoderProvider: ILazyProvider<Worker>
        ) {
            this.unstartedTrack = <IRecorderTrack>this.$q.defer();
            this.unstartedTrack.reject();
        };

        createRecorder = () => {
            if (!this.deferredCreate) {
                this.deferredCreate = this.$q.defer();
                (this.internalCreateRecorder || this.deferredCreate.resolve)();
            }
            return this.$q
                .all([this.encoderProvider.get(), this.deferredCreate.promise])
                .then(
                (resolved: {}[]) => {
                    this.encoder = <Worker>resolved[0];
                    // If multiple identical EventListeners are registered on the same EventTarget with the same parameters, the duplicate instances are discarded.
                    // Be carefull, the handler must not be a closure, otherwise we would end up with duplicate handler calls on each event.
                    this.encoder.addEventListener('message', this.encoderEventListener);
                    return this;
                },
                (reason) => { toastr.error(reason.message); }
                );
        };

        startRecorder = () => {
            var deferred = this.$q.defer();
            deferred.resolve();
            return deferred.promise;
        };

        stopRecorder = () => {
            this.stopTrack();
        };

        startTrack = (trackId: string) => {
            this.stopTrack().finally(() => {
                if (trackId) {
                    (this.internalStartTrack || angular.noop)();
                    this.track = <IRecorderTrack>this.$q.defer();
                    this.track.trackId = trackId;
                    this.track.isRecording = true;
                }
            });
        };

        stopTrack = (data?: ArrayBuffer) => {
            if (this.track) {
                this.track.isRecording = false;
                (this.internalStopTrack || angular.noop)(data);
            }
            return (this.track || this.unstartedTrack).promise;
        };

    } // end of class RecorderBase

    export abstract class Mp3Recorder extends RecorderBase {
        // Do not change the rate values until tested. We need to match the Flash and lamejs features. 
        // lamejs cannot encode 48kHz -> 32kbps. 48kHz -> 64kbps works well. It is probably related to the lowpass frequency in LAME.
        // On LAME's sample rate and bitrate compatability see +http://sourceforge.net/p/audacity/mailman/message/27801280/
        // See Flash's sample rates at +http://help.adobe.com/en_US/as3/dev/WS5b3ccc516d4fbf351e63e3d118a9b90204-7d1d.html#WS5b3ccc516d4fbf351e63e3d118a9b90204-7d0d  
        static SampleRate = 16000; // Hz
        static BitRate = 32; // kbps
        // AudioContext has a hardware-dependant fixed sample rate, we resample in the worker.
        originalSampleRate: number;

        internalCreateRecorder: () => void;
        encodedPages: Uint8Array[] = [];
        totalLength: number = 0;

        constructor(
            public $q: angular.IQService,
            public $timeout: angular.ITimeoutService,
            public $window: angular.IWindowService,
            public encoderProvider: ILazyProvider<Worker>
        ) {
            super($q, $timeout, $window, encoderProvider);
        };

        private initEncoder = () => {
            var config: app.IEncoderConfig = {
                command: 'init',
                channels: 1,
                originalSampleRate: this.originalSampleRate,
                sampleRate: Mp3Recorder.SampleRate,
                bitRate: Mp3Recorder.BitRate,
            };
            this.encoder.postMessage(config);
        };

        protected encode = (samples: number[]) => {
            if (this.track && this.track.isRecording) {
                // Flash and getUserMedia send PCM32 as native JS numbers representing floats in range -1.0...1.0 .
                var array = new Float32Array(samples);
                // Typed arrays can be passed directly as transferrable objects without serializing/deserializing by using the special signature of postMessage()
                this.encoder.postMessage(array, [array.buffer]);
            }
        };

        private storeEncodedPage = (page: Uint8Array, isFinal?: boolean) => {
            this.encodedPages.push(page);
            this.totalLength += page.byteLength;

            if (isFinal) {
                if (this.track) {
                    var outputData = new Uint8Array(this.totalLength);
                    var outputIndex = 0;
                    for (var i = 0; i < this.encodedPages.length; i++) {
                        outputData.set(this.encodedPages[i], outputIndex);
                        outputIndex += this.encodedPages[i].length;
                    }
                    var blob = new Blob([outputData], { type: 'audio/mpeg' });
                    this.tracks[this.track.trackId] = blob;
                    this.track.resolve();
                    this.track = null;
                }
            }
        };

        encoderEventListener = (ev: MessageEvent) => {
            if (ev.data.byteLength) {
                this.storeEncodedPage(ev.data);
            } else
                if (ev.data.isFinal) {
                    this.storeEncodedPage(ev.data.page, ev.data.isFinal);
                }
        };

        internalStartTrack = () => {
            this.initEncoder();
            this.encodedPages = [];
            this.totalLength = 0;
        };

        internalStopTrack = () => {
            this.encoder.postMessage({ command: 'finish' });
        }

    } // end of class RecorderBase

    // Parameters are determined by RecorderDesktop
    export type RecorderConstructor = new (
        $q: angular.IQService,
        $timeout: angular.ITimeoutService,
        $window: angular.IWindowService,
        encoderProvider: ILazyProvider<Worker>
    ) => IRecorder;

    export class RecorderProvider implements angular.IServiceProvider, app.IServiceProvider<RecorderConstructor> {
        static ServiceName = 'appRecorderProvider';

        recorderCtor: RecorderConstructor;

        configure = (param: RecorderConstructor) => {
            this.recorderCtor = param;
        };

        $get = [app.ngNames.$q, app.ngNames.$timeout, app.ngNames.$window, EncoderFabric.ServiceName,
            (
                $q: angular.IQService,
                $timeout: angular.ITimeoutService,
                $window: angular.IWindowService,
                encoderProvider: ILazyProvider<Worker>
            ) => {
                return <ILazyProvider<IRecorder>>{
                    get: () => {
                        var recorder = new this.recorderCtor($q, $timeout, $window, encoderProvider);
                        return recorder.createRecorder();
                    }
                };
            }
        ];

    } // end of class RecorderProvider

    export class States {
        static Initial = ''; // The default state set up by UI-Router when it initializes itself. We do not control it.
        static Ready = 'Ready';
        static Starting = 'Starting';
        static Idle = 'Idle';
        static Playing = 'Playing';
        static Repeating = 'Repeating';
        static Recording = 'Recording';
        static Pause = 'Pause';
        static Stopped = 'Stopped';
        static Uploading = 'Uploading';
        static Uploaded = 'Uploaded';
        static Error = 'Error';
    } // end of class States 

    export class StateConfig {

        static $inject = [app.ngNames.$stateProvider/*, ReadyStateResolverFactory.ServiceName*/];
        constructor(
            $stateProvider: angular.ui.IStateProvider
        ) {
            var createState = (name: string, toStates: string | string[]) => {
                return <angular.ui.IState>{
                    name: name,
                    data: {
                        toStates: angular.isArray(toStates) ? toStates : [toStates],
                    },
                };
            };

            // BTW. Child states inherit resolved values from the parent.
            var ready: angular.ui.IState = {
                name: States.Ready,
                data: {
                    toStates: [States.Starting],
                },
                resolve: {
                    card: [app.exercises.CardProvider.ServiceName, (provider: ILazyProvider<ICard>) => {
                        return provider.get();
                    }],
                    player: [app.exercises.PlayerProvider.ServiceName, (provider: ILazyProvider<app.exercises.IPlayer>) => {
                        return provider.get();
                    }],
                    recorder: [RecorderProvider.ServiceName, (provider: ILazyProvider<IRecorder>) => {
                        return provider.get();
                    }],
                },
            };

            $stateProvider
                .state(ready)
                .state(createState(States.Starting, [States.Idle]))
                .state(createState(States.Idle, [States.Playing, States.Pause, States.Stopped]))
                .state(createState(States.Playing, [States.Idle, States.Recording, States.Stopped]))
                .state(createState(States.Recording, [States.Idle, States.Repeating, States.Stopped]))
                .state(createState(States.Repeating, [States.Recording, States.Stopped]))
                .state(createState(States.Pause, [States.Idle, States.Stopped]))
                .state(createState(States.Uploading, [States.Uploaded]))
                .state(createState(States.Uploaded, []))
                .state(createState(States.Stopped, [States.Uploading]))
                .state(createState(States.Error, []))
            ;
        };

    }; // end of class StateConfigBase   

    export class StateChangeHandlers {

        static $inject = [app.ngNames.$rootScope];
        constructor(
            $rootScope: IAppRootScopeService
        ) {
            //Enforce allowed transitions between states. The very initial state is an internal empty one.
            //By the way, ui.router uses a kind of a hierarchical state tree, not a classical finite state machine.
            $rootScope.$on('$stateChangeStart', (event, toState: angular.ui.IState, toParams, fromState: angular.ui.IState, fromParams) => {
                //console.log(moment.utc().toISOString(), fromState.name, ' > ', toState.name);
                var toStates: string[] = (fromState.data && fromState.data.toStates) || [toState.name];
                toStates.push(States.Error);
                /* I have observed a race condition in Android Chrome. 
                A "Next question" click initiates the transition to Idle which takes a couple seconds for finishing the track. 
                The execution is lose coupled with the Worker via messages (Worker executes in a background thread.)
                Then the "Stop" button is clicked in a second. It has the UI priority and executes instantly. It forces
                the transition to Stopped and assigns item = null. Then the interrupted code sequence continues and calls forward()
                and assigns item != null. Then it is heading to Playing, but conflicting here.                    
                */
                if (toStates.indexOf(toState.name) === -1) {
                    event.preventDefault();
                    console.log('Wrong state transition.');
                }
            });
            $rootScope.$on('$stateChangeError', (event, toState: angular.ui.IState, toParams, fromState: angular.ui.IState, fromParams, error) => {
                // A good example is at +http://stackoverflow.com/questions/22936865/handling-error-in-ui-routers-resolve-function-aka-statechangeerror-passing-d
                event.preventDefault();
                // message is a custom field on the object passed to the rejected promise.
                if (angular.isObject(error) && angular.isString(error.message)) {
                    toastr.error(error.message);
                }
            });
        }
    }; // end of class StateChangeHandlers

    cardPlayerProviderConfig.$inject = [app.exercises.PlayerProvider.ServiceName + 'Provider'];
    export function cardPlayerProviderConfig(provider: app.IServiceProvider<string>) {
        var cardId = app['cardIdParam'];
        var url = app.getBlobUrl('ielts-speaking', cardId + '.mp3');
        provider.configure(url);
    };

} // end of module app.exercisesIelts
