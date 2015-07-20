module app.exercisesIelts {

    interface IRecordingEvent {
        time: number;
        event: string;
    }

    export class SpeakingDesktop extends app.exercisesIelts.IeltsBase {

        flashFound: boolean = true;
        currentPart: number = -1;

        recording: boolean = false;
        encoded: boolean = false;
        uploading: boolean = false;
        events: IRecordingEvent[] = [];
        startTime: number;

        static $inject = [app.ngNames.$document, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$scope, app.ngNames.$timeout, app.ngNames.$window];

        constructor(
            private $document: angular.IDocumentService,
            $http: angular.IHttpService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            private $timeout: angular.ITimeoutService,
            private $window: angular.IWindowService
            ) {
            /* ----- Constructor  ----- */
            super($http, $modal, $scope);
           // this.cardType = 'CDIS__';

            this.createRecorder();
            /* ----- End of constructor  ----- */
        } // end of ctor  

        createRecorder = () => {
            var swfUrl = this.$window.location.protocol + '//' + this.$window.location.host + '/content/audior/Audior.swf';
            var idOfElementToReplace = 'toBeReplacedByAudiorSwfObject';
            var width = 520; //600
            var height = 142;
            var swfVersion = '10.2.0';
            var expressInstallSwfUrl = '';

            var flashvars = {
                lstext: 'Loading...',
                recorderId: Date.now().toString(),
                userId: 0,
                licenseKey: '656e676c697368617269756d2e636f6d3f617a757265aurc8be77656273697465732e6e65743f6c6f63616c686f7374aurc0c2c4baceba198',
                settingsFile: 'audior_settings.xml',
            };

            var params = {
                quality: 'high',
                bgcolor: '#ffffff',
                allowscriptaccess: 'sameDomain',
                allowfullscreen: 'false',
                base: '/content/audior/'
            };

            var attributes = {
                id: 'myAudior',
                name: 'audior',
                align: 'middle',
            };

            var callbackFn = (event) => {
                if (!event.success) {
                    this.flashFound = false;
                    toastr.error('Adobe Flash version 10.2.0 or above not found.');
                }
            };

            var wnd = <any>this.$window;

            var swfObject = wnd.swfobject;
            swfObject.embedSWF(swfUrl, idOfElementToReplace, width, height, swfVersion, expressInstallSwfUrl, flashvars, params, attributes, callbackFn);

            wnd.btRecordClick = (recorderId) => {
                this.recording = true;
                this.setPart(0);
                this.startTime = Date.now(),
                this.addEvent('record');
            };

            wnd.onMicAccess = (allowed, recorderId) => {
                this.addEvent('mic ' + allowed);
            };

            wnd.btStopClick = (recorderId) => {
                this.recording = false;
                this.setPart(-1);
                this.addEvent('stop');
            };

            wnd.onEncodingDone = (duration, recorderId) => {
                this.encoded = true;
                this.$scope.$apply();
            };

            wnd.onUploadDone = (success, recordName, duration, recorderId) => {
                if (success) {
                    app.ngHttpPut(this.$http,
                        app.exercisesApiUrl('recording_details'),
                        {
                            recorderId: recorderId,
                            recordName: recordName,
                            duration: duration,
                            title: this.card.title,
                            cardId: this.card.id,
                        },
                        (data) => {
                            if (data) {
                                this.$window.location.assign('/exercises/view/' + data);
                            }
                        });
                }
            };

            //function onFlashReady(recorderId) {
            //};

        }

        getItems = (part: number) => {
            return this.card
                ? this.card.items.filter((i) => { return this.getPart(i) === part; })
                : null;
        };

        getPart = (item: ICardItem) => {
            return item && +item.position.slice(0, 1);
        };

        addEvent = (event: string) => {
            this.events.push({
                time: Date.now() - this.startTime,
                event: event
            });
            if (!this.$scope.$$phase)
                this.$scope.$apply();
        };

        canSave = () => {
            return app.isAuthenticated() && this.encoded && !this.uploading;
        };

        save = () => {
            this.uploading = true;
            //var doc = <Document>(<any>this.$document[0]);
            //var audior = <any>doc.getElementById('myAudior');
            //audior.save();            
            (<any>this.$document[0]).myAudior.save();
        };

        resetCard = () => {
            this.currentPart = 0;
        };

        setPart = (part: number) => {
            this.addEvent('part' + part);
            this.currentPart = part;
            app.soundHit();

            var timeout = null;
            switch (part) {
                case 0:
                case 2:
                    timeout = 300000;
                    break;
                case 1:
                    timeout = 180000;
                    break;
                default:
                    timeout = null;
            }
            if (timeout && this.recording) {
                this.$timeout(() => {
                    if ((this.currentPart === part) && this.recording) {
                        this.setPart(part + 1);
                    }
                }, timeout);
                // part 2 preparation
                if (part == 1) {
                    this.$timeout(() => {
                        if (this.currentPart == part) {
                            app.soundBeep();
                        }
                    }, 60000);
                }
            }
        };

    } // end of class Ctrl    

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ngSanitize', 'ui.router'])
        .controller('SpeakingDesktop', SpeakingDesktop)
    ;

} // end of module


