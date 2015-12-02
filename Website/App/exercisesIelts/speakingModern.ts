// Extend the standard declararions.
interface Navigator {
    getUserMedia: (constraints, successCallback, errorCallback) => void;
}

interface AudioContext {
    createMediaStreamSource: (stream) => AudioNode; // Actually MediaStreamAudioSourceNode
}

module app.exercisesIelts {

    class RecorderModern extends Mp3Recorder {

        audioContext: AudioContext;
        stream: any;
        processorNode: ScriptProcessorNode;

        // No injection. This object is created by a factory which has got the services injected and passes them directly to the ctor.
        constructor(
            $q: angular.IQService,
            $timeout: angular.ITimeoutService,
            $window: angular.IWindowService,
            encoderProvider: ILazyProvider<Worker>
        ) {
            super($q, $timeout, $window, encoderProvider);
        };

        internalCreateRecorder = () => {
            AudioContext = AudioContext || (<any>window).webkitAudioContext;
            navigator.getUserMedia = navigator.getUserMedia || (<any>navigator).webkitGetUserMedia || (<any>navigator).mozGetUserMedia;

            if (AudioContext && navigator.getUserMedia) {
                try {
                    this.audioContext = new AudioContext();
                    this.originalSampleRate = this.audioContext.sampleRate;
                    this.deferredCreate.resolve();
                } catch (e) {
                    this.deferredCreate.reject({ message: (e.message || 'Error.'), });
                }
            }
            else {
                this.deferredCreate.reject({ message: 'Recording is not supported in this browser.', });
            }
        };

        startRecorder = () => {
            var deferred = this.$q.defer();
            var successCallback = (stream) => {
                this.stream = stream;
                var sourceNode = this.audioContext.createMediaStreamSource(stream);
                this.processorNode = this.audioContext.createScriptProcessor(4096, 1, 1);
                this.processorNode.onaudioprocess = (ev: AudioProcessingEvent) => {
                    var buffer = ev.inputBuffer.getChannelData(0);
                    this.encode(buffer);
                };
                sourceNode.connect(this.processorNode);
                // Do not move up. The node starts receiving samples as soon as it is connected to the destination, before connected to a source, even while the confirmation dialog is being shown.
                this.processorNode.connect(this.audioContext.destination);
                deferred.resolve();
            };
            var errorCallback = (e: DOMException) => {
                console.log(2);
                var msg = (e.name || e.message);
                if (msg === 'PermissionDeniedError') {
                    msg += ' . Change the microphone permissions by reloading the page or by clicking the icon in the address bar or by changing the site settings in the browser menu.'
                }
                deferred.reject({ message: msg, code: e.code, }); // PermissionDeniedError, NotFoundError
            };
            navigator.getUserMedia({ audio: true, video: false, }, successCallback, errorCallback);
            return deferred.promise;
        };

        stopRecorder = () => {
            this.stopTrack();
            if (this.stream) {
                this.processorNode.onaudioprocess = null;
                this.processorNode.disconnect(); // Without this, the AudioContext keeps sending (empty?) samples to onaudioprocess.
                var tracks: any[] = this.stream.getTracks();
                tracks.forEach((i) => { i.stop(); }); // MediaStreamTrack.stop() does not hide the mic indicator in Firefox.
                // 'MediaStream.stop()' is deprecated in Chrome but is the only way available in Firefox.
                if (this.stream.stop) {
                    this.stream.stop();
                }
                this.stream = null;
            }
        };

    } // end of class RecorderModern

    // Angular uses the suffix as a naming convention to configure the povider.
    recorderProviderConfig.$inject = [RecorderProvider.ServiceName + 'Provider'];
    function recorderProviderConfig(provider: app.IServiceProvider<RecorderConstructor>) {
        provider.configure(RecorderModern);
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
