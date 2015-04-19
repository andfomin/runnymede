module app.ielts {

    export class SpeakingDesktop extends app.ielts.IeltsBase {

        recorderId: string;
        flashFound: boolean = true;

        static $inject = [app.ngNames.$document, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$scope, app.ngNames.$window];

        constructor(
            private $document: angular.IDocumentService,
            $http: angular.IHttpService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            private $window: angular.IWindowService
            ) {
            /* ----- Constructor  ----- */
            super($http, $modal, $scope);

            this.recorderId = Date.now().toString();
            this.createRecorder();
            /* ----- End of constructor  ----- */
        } // end of ctor  

        createRecorder = () => {
            var swfUrl = this.$window.location.protocol + '//' + this.$window.location.host + '/content/audior/Audior.swf';
            var idOfElementToReplace = 'toBeReplacedByAudiorSwfObject';
            var width = 600;
            var height = 142;
            var swfVersion = '10.2.0';
            var expressInstallSwfUrl = '';

            var flashvars = {
                lstext: 'Loading...',
                recorderId: this.recorderId,
                userId: 0,
                licenseKey: '656e676c697368617269756d2e636f6d3f617a757265aurc8be77656273697465732e6e65743f6c6f63616c686f7374aurc0c2c4baceba198',
                //settingsFile: "audior_settings.xml", // default value
            };

            var params = {
                quality: 'high',
                bgcolor: '#ffffff',
                allowscriptaccess: 'sameDomain',
                allowfullscreen: 'false',
                base: '/content/audior/'
            };

            var attributes = {
                id: 'Audior',
                name: 'Audior',
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

            wnd.onEncodingDone = (duration, recorderId) => {
                (<any>this.$document[0]).Audior.save();
                this.$scope.$apply();
            };

            wnd.onUploadDone = (success, recordName, duration, recorderId) => {
                if (success) {
                    app.ngHttpPut(this.$http,
                        app.exercisesApiUrl('recording_title'),
                        {
                            recorderId: recorderId,
                            duration: duration,
                            title: this.getSecondaryTitle(),
                        },
                        (data) => {
                            if (data && data.exerciseId) {
                                this.$window.location.assign('/exercises/view/' + data.exerciseId);
                            }
                        });
                }
            };

            //(<any>this.$window).btRecordClick = (recorderId) => {
            //}

            //function onMicAccess(allowed, recorderId) {
            //};

            //function onFlashReady(recorderId) {
            //};
        }

    } // end of class Ctrl    

    angular.module(app.myAppName, ['ngSanitize', 'ui.bootstrap', 'angular-loading-bar'])
        .controller('SpeakingDesktop', SpeakingDesktop)
    ;

} // end of module


