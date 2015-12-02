module app.exercisesIelts {

    interface IFWRecorder {
        isMicrophoneAccessible: () => void;
        configure: (rate?: number, gain?: number, silenceLevel?: number, silenceTimeout?: number) => void;
        permitPermanently: () => void;
        record: (soundName: string) => void;
        stopRecording: () => void;
        observeSamples: () => void;
        stopObservingSamples: () => void;
    }

    // based on +https://github.com/michalstocki/FlashWavRecorder/blob/master/html/js/recorder.js
    class RecorderDesktop extends Mp3Recorder {

        private static SwfElementId = 'myRecorderSwfObject';
        private static BoxElementId = 'myRecorderBox';
        private static BoxClass = 'my-visible';

        private fwRecorder: IFWRecorder;
        private deferredStarted: angular.IDeferred<any>;

        // No injection. This object is created by a factory which has got the services injected and passes them directly to the ctor.
        constructor(
            $q: angular.IQService,
            $timeout: angular.ITimeoutService,
            $window: angular.IWindowService,
            encoderProvider: ILazyProvider<Worker>
        ) {
            super($q, $timeout, $window, encoderProvider);
            this.originalSampleRate = Mp3Recorder.SampleRate;
        };

        internalCreateRecorder = () => {
            // The event handler name is hard-coded in src/main/flashwavrecorder/RecorderJSInterface.as .
            (<any>this.$window).fwr_event_handler = this.flashEventHandler;

            var swfUrl = app['swfUrl'];
            var idOfElementToReplace = 'toBeReplacedByRecorderSwfObject';
            var width = '240';
            var height = '160';
            var swfVersion = '11.0.0';
            var installSwfUrl = '';
            var flashvars = null;
            var params = null;
            var attributes = { id: RecorderDesktop.SwfElementId, };
            var callbackFn = (callbackObj: swfobject.ICallbackObj) => {
                if (!callbackObj.success) {
                    this.deferredCreate.reject({ message: 'Adobe Flash version 11.0.0 or above not found.', });
                }
            };
            swfobject.embedSWF(swfUrl, idOfElementToReplace, width, height, swfVersion, installSwfUrl, flashvars, params, attributes, callbackFn);
        };

        private getElement = (elementId: string) => {
            var element = this.$window.document.getElementById(elementId);
            if (!element) {
                this.deferredCreate.reject({ message: 'Element not found. ' + elementId });
            }
            return element;
        };

        private flashEventHandler = (eventName: string, arg1?: any) => {
            console.log(eventName);

            if (!this.fwRecorder) {
                this.fwRecorder = <IFWRecorder><any>this.getElement(RecorderDesktop.SwfElementId);
            }

            switch (eventName) {
                case 'ready':
                    this.deferredCreate.resolve();
                    break;
                case 'no_microphone_found':
                    this.deferredStarted && this.deferredStarted.reject({ message: 'Microphone not found.', });
                    break;
                case 'microphone_connected':
                    // Be careful, configure() is initially called internally with default parameters just before this event is sent. src/main/flashwavrecorder/RecorderJSInterface.as
                    this.fwRecorder.configure(this.originalSampleRate / 1000); // Expects kHz
                    break;
                case 'microphone_not_connected':
                    this.deferredStarted && this.deferredStarted.reject({ message: 'Please reload the page to enable microphone access.', });
                    break;
                case 'permission_panel_closed':
                    angular.element(this.getElement(RecorderDesktop.BoxElementId)).removeClass(RecorderDesktop.BoxClass);
                    if (this.fwRecorder.isMicrophoneAccessible()) {
                        this.$timeout(() => { this.fwRecorder.record(Math.random().toString()); }, 0, false);
                    }
                    break;
                case 'recording':
                    // The event confirming this command is 'observing_level'. It is a copy-paste mistake in the source code.
                    this.fwRecorder.observeSamples();
                    break;
                case 'recording_stopped':
                    // The event confirming this command is 'observing_level_stopped'. It is a copy-paste mistake in the source code.
                    this.fwRecorder.stopObservingSamples();
                    break;
                case 'microphone_samples':
                    if (this.deferredStarted) {
                        var deferred = this.deferredStarted;
                        this.deferredStarted = null;
                        this.$timeout(() => { deferred.resolve(); }, 0, false);
                    }
                    this.encode(arg1);
                    break;
            }
        };

        startRecorder = () => {
            this.deferredStarted = this.$q.defer();
            // In Chrome, Flash function start working stable some time after the "ready" event, so we may catch an exception if the user presses Start just after the button got enabled. That exception has been observed in Chrome.
            // Everything works fine after the "microphone_connected" event.
            // If access not allowed by default, we don't get the initial "microphone_connected" and we have no idea, when isMicrophoneAccessible() becomes available.            
            // The sad fact is that even if we avoided calling isMicrophoneAccessible(), we would get an exception on permitPermanently() call anyway, even after waiting for 2 seconds. In Chrome.
            if (this.fwRecorder.isMicrophoneAccessible()) {
                this.fwRecorder.record(Math.random().toString());
            } else {
                angular.element(this.getElement(RecorderDesktop.BoxElementId)).addClass(RecorderDesktop.BoxClass);
                this.$timeout(() => { this.fwRecorder.permitPermanently(); }, 0, false);
            }
            return this.deferredStarted.promise;
        };

        stopRecorder = () => {
            this.fwRecorder.stopRecording();
        };

    } // end of class RecorderDesktop

    // Angular uses the suffix as a naming convention to configure the povider.
    recorderProviderConfig.$inject = [RecorderProvider.ServiceName + 'Provider'];
    function recorderProviderConfig(provider: app.IServiceProvider<RecorderConstructor>) {
        provider.configure(RecorderDesktop);
    };

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ui.router', 'ngSanitize'])
        //.constant(ReadyStateResolverFactory.ServiceName, ReadyStateResolverFactory)
        .config(app.exercisesIelts.cardPlayerProviderConfig)
        .config(recorderProviderConfig)
        .config(StateConfig)
        .service(app.exercises.CardProvider.ServiceName, app.exercises.CardProvider)
        .provider(app.exercises.PlayerProvider.ServiceName, app.exercises.PlayerProvider)
        .service(EncoderFabric.ServiceName, EncoderFabric)
        .provider(RecorderProvider.ServiceName, RecorderProvider)
        .run(StateChangeHandlers)
        .controller('Speaking', app.exercisesIelts.SpeakingBase)
    ;

} // end of module exercisesIelts
