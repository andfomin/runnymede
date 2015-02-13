module app.exercises {

    export class RecordSpeech {

        ActivityTelling: string = 'Telling'; // const
        ActivityRetelling: string = 'Retelling'; // const

        private busy: boolean;
        selectedActivity: string;
        categoriesL2: app.library.ICategory[] = [];
        categoriesL3: app.library.ICategory[] = [];
        selectedL2: app.library.ICategory = null;
        selectedL3: app.library.ICategory = null;
        opened: boolean = false;
        recordingTitle: string = null;
        cards: ITopicCard[] = null; // ITopicCard is declared in topics-cardList.ts
        selectedCard: ITopicCard;
        recorderId: string;
        mobile: boolean = false;
        flashFound: boolean = true;
        encodingDone: boolean = false;
        uploading: boolean = false;

        myTest: string;

        static $inject = [app.ngNames.$scope, app.ngNames.$rootScope, app.ngNames.$http, app.ngNames.$window, app.ngNames.$document, app.ngNames.$modal];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $rootScope: ng.IRootScopeService,
            private $http: ng.IHttpService,
            private $window: ng.IWindowService,
            private $document: ng.IDocumentService,
            private $modal: ng.ui.bootstrap.IModalService
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;

            this.recorderId = Date.now().toString();

            this.mobile = app.isMobileDevice() && captureSupported('audio/*');
            // ng-change does not support input["file"]. We attach a handler to the native event on the element.
            (<any>RecordSpeech).onFileChange = () => { $scope.$apply(); };

            this.createAudiorRecorder();

            $scope.$on(app.library.ResourceList.Selected, (event, args) => { this.recordingTitle = args && args.resource && args.resource.title; });

            var topicsCategory = app.arrFind(app.library.Categories, (i) => { return i.id === '189_'; }); // "Topics"
            if (topicsCategory) {
                var parentId = topicsCategory.id;
                this.categoriesL3 = app.library.Categories.filter((i) => { return i.parentId === parentId; });
            }
            /* ----- End of constructor  ----- */
        } // end of ctor  

        clearList = () => {
            this.$rootScope.$broadcast(app.library.ResourceList.Clear);
            this.cards = null;
            this.selectCard(null);
        };

        selectActivity = (activity: string) => {
            this.selectedActivity = activity;
            this.selectL3(null);
        };

        selectL3 = (c3: app.library.ICategory) => {
            this.selectedL3 = c3;
            this.clearList();
            if (this.selectedL3) {
                if (this.isTelling()) {
                    this.cards = TopicCards.filter(i => { return i.categoryId === this.selectedL3.id; });
                }
                if (this.isRetelling()) {
                    if (!this.busy) {
                        this.busy = true;
                        app.ngHttpGet(this.$http,
                            app.libraryApiUrl('common'),
                            {
                                categoryId: this.selectedL3.id,
                            },
                            (data) => {
                                if (data && angular.isArray(data.value)) {
                                    this.$rootScope.$broadcast(app.library.ResourceList.Display, { resources: data.value });
                                }
                            },
                            () => { this.busy = false; }
                            );
                    }
                }
            }
        };

        selectCard = (card) => {
            this.selectedCard = card;
            this.recordingTitle = this.selectedCard && this.selectedCard.title;
        }

        isRetelling = () => {
            return this.selectedActivity === this.ActivityRetelling;
        };

        isTelling = () => {
            return this.selectedActivity === this.ActivityTelling;
        };

        fileReady = () => {
            var input = <any>angular.element(this.$document[0].querySelector('#fileInput'));
            var files = input && input[0].files;
            return files && (files.length == 1) && !this.uploading;
        };

        onSubmit = () => {
            this.uploading = true;
        };

        canSave = () => {
            return this.encodingDone && app.isAuthenticated();
        };

        save = () => {
            (<any>this.$document[0]).Audior.save();
        };

        createAudiorRecorder = () => {
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

            if (!this.mobile) {
                var swfObject = (<any>this.$window).swfobject;
                swfObject.embedSWF(swfUrl, idOfElementToReplace, width, height, swfVersion, expressInstallSwfUrl, flashvars, params, attributes, callbackFn);

                (<any>this.$window).btRecordClick = (recorderId) => {
                    this.encodingDone = false;
                };

                (<any>this.$window).onEncodingDone = (duration, recorderId) => {
                    this.encodingDone = true;
                    this.$scope.$apply();
                };

                (<any>this.$window).onUploadDone = (success, recordName, duration, recorderId) => {
                    if (success) {
                        app.ngHttpPut(this.$http,
                            app.exercisesApiUrl('recording_title'),
                            {
                                recorderId: recorderId,
                                duration: duration,
                                title: this.recordingTitle,
                            },
                            (data) => {
                                if (data && data.exerciseId) {
                                    this.$window.location.assign('/exercises/view/' + data.exerciseId);
                                }
                            });
                    }
                };
            }

            //function onMicAccess(allowed, recorderId) {
            //};

            //function onFlashReady(recorderId) {
            //};
        }

    } // end of class Ctrl    

    angular.module(app.myAppName, ['ui.bootstrap', 'angular-loading-bar'])
        .controller('RecordSpeech', RecordSpeech)
        .controller('ResourceList', app.library.ResourceList);

} // end of module


