module app.exercisesIelts {

    export class SpeakingCapture extends app.exercisesIelts.SpeakingBase {

        static $inject = [app.ngNames.$http, app.ngNames.$interval, app.ngNames.$rootScope, app.ngNames.$scope, ngNames.$state, app.ngNames.$window];
        constructor(
            $http: angular.IHttpService,
            $interval: angular.IIntervalService,
            $rootScope: angular.IRootScopeService,
            $scope: app.IScopeWithViewModel,
            $state: angular.ui.IStateService,
            $window: angular.IWindowService
        ) {
            /* ----- Constructor  ----- */
            super($http, $interval, $rootScope, $scope, $state, $window);
            /* ----- End of constructor  ----- */
        } // end of ctor 

        onFileChange = (file: File) => {
            // 14 min of QickTime video is approximatelly 75MB.
            if (file.size > 75 * 1024 * 1024) {
                toastr.error('The file size is too big.');
                return;
            }
            if (file.type !== 'video/quicktime') {
                toastr.error('Unsupported file type.');
                return;
            }

            var reader = new FileReader();
            reader.onload = () => {
                this.recorder.stopTrack(reader.result)
                    .then(() => {
                        // We have got a transformed file. Release the original video file.
                        var form = <HTMLFormElement>this.$window.document.getElementById('myFileForm');
                        form.reset();
                        this.go(States.Idle);
                    });
            }
            reader.readAsArrayBuffer(file);
        };

    } // end of class SpeakingCapture

    export class QtRecorder extends RecorderBase {

        constructor(
            public $q: angular.IQService,
            public $timeout: angular.ITimeoutService,
            public $window: angular.IWindowService,
            public encoderProvider: ILazyProvider<Worker>
        ) {
            super($q, $timeout, $window, encoderProvider);
        };

        encoderEventListener = (ev: MessageEvent) => {
            if (this.track && ev.data && ev.data.byteLength) {
                var blob = new Blob([ev.data], { type: 'video/quicktime' });
                this.tracks[this.track.trackId] = blob;
                this.track.resolve();
                this.track = null;
            }
        };

        internalStopTrack = (data?: ArrayBuffer) => {
            if (data) {
                // Typed arrays can be passed directly as transferrable objects without serializing/deserializing by using the special signature of postMessage()
                this.encoder.postMessage(data, [data]);
            }
            else {
                this.track.reject();
                this.track = null;
            }
        };

    } // end of class QtRecorder

    // Angular uses the suffix as a naming convention to configure the povider.
    recorderProviderConfig.$inject = [RecorderProvider.ServiceName + 'Provider'];
    function recorderProviderConfig(provider: app.IServiceProvider<RecorderConstructor>) {
        provider.configure(QtRecorder);
    };

    MyOnChange.DirectiveName = 'myOnchange';
    function MyOnChange(): angular.IDirective {
        return <angular.IDirective>{
            restrict: 'A',
            link: function (scope: angular.IScope, element: angular.IAugmentedJQuery, attributes: angular.IAttributes) {
                var handler = (event: Event) => {
                    var onFileChange = scope.$eval(attributes['myOnchange']);
                    //var input = <HTMLInputElement>element[0];
                    var input = <HTMLInputElement>event.target;
                    var file = input && input.files && input.files[0];
                    if (onFileChange && file) {
                        scope.$evalAsync(() => {
                            onFileChange(file);
                        });
                    }
                };
                element.on('change', handler);
            }
        };
    };

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ui.router', 'ngSanitize'])
        .directive(MyOnChange.DirectiveName, MyOnChange)
        //.constant(ReadyStateResolverFactory.ServiceName, ReadyStateResolverFactory)
        .config(app.exercisesIelts.cardPlayerProviderConfig)
        .config(recorderProviderConfig)
        .config(StateConfig)
    //.config(app.DisableAngularDebugInfo)
        .service(app.exercises.CardProvider.ServiceName, app.exercises.CardProvider)
        .provider(app.exercises.PlayerProvider.ServiceName, app.exercises.PlayerProvider)
        .service(EncoderFabric.ServiceName, EncoderFabric)
        .provider(RecorderProvider.ServiceName, RecorderProvider)
        .run(StateChangeHandlers)
        .controller('Speaking', SpeakingCapture)
    ;

} // end of module
