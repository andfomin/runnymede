module app.exercisesIelts {

    export class Writing extends app.exercisesIelts.IeltsBase {

        supported: boolean = false;
        allPages = [0, 1, 2, 3, 4];
        pageCount: number = 3;
        rotations = [0, 0, 0, 0, 0];
        uploading: boolean = false;

        static $inject = [app.ngNames.$http, app.ngNames.$modal, app.ngNames.$scope, app.ngNames.$timeout];

        constructor(
            $http: angular.IHttpService,
            $modal: angular.ui.bootstrap.IModalService,
            $scope: app.IScopeWithViewModel,
            private $timeout: angular.ITimeoutService
            ) {
            /* ----- Constructor  ----- */
            super($http, $modal, $scope);

            this.supported = app.isMobileDevice() && captureSupported('image/jpeg');
            // ng-change does not support input["file"]. We attach a handler to the native event on the element.
            (<any>Writing).onFileChange = this.onFileChange;
            /* ----- End of constructor  ----- */
        } // end of ctor  

        private pages = () => {
            return this.allPages.slice(0, this.pageCount);
        };

        onFileChange = (event: Event) => {
            var input = <HTMLInputElement>event.target;
            if (input.files.length > 1) {
                toastr.warning('Only one file per placeholder please!');
            }
            this.drawImage(input);
            this.$timeout(() => { this.$scope.$apply(); }, 100);
        };

        rotate = (i: number) => {
            this.drawImage(this.getInput(i));
        };

        drawImage = (input: HTMLInputElement) => {
            var file = (input.files.length == 1) && input.files[0];
            if (file) {
                var blobUrl = URL.createObjectURL(file);
                var image = new Image();

                image.onload = () => {
                    URL.revokeObjectURL(blobUrl);

                    var size = 100;// Canvas's width and height are set equal to 100 in HTML. BTW there is also canvas.style.width
                    var imgSize = Math.max(image.width, image.height);
                    var scale = size / imgSize;
                    var imgWidth = image.width * scale;
                    var imgHeight = image.height * scale;

                    var canvas = <HTMLCanvasElement>(document.getElementById('canvas' + input.id));
                    var context = canvas.getContext('2d');
                    context.clearRect(0, 0, size, size);

                    var i = input.id.substring(4);
                    var rotate = + this.rotations[i]; // The radio control stores strings
                    switch (rotate) {
                        case 0:
                            context.drawImage(image, 0, 0, image.width, image.height,(size - imgWidth) / 2,(size - imgHeight) / 2, imgWidth, imgHeight);
                            break;
                        case -1:
                        case 1:
                            context.save();
                            context.translate(size / 2, size / 2);
                            context.rotate(rotate * Math.PI / 2);
                            context.translate(-size / 2, -size / 2);
                            context.drawImage(image, 0, 0, image.width, image.height,(size - imgWidth) / 2,(size - imgHeight) / 2, imgWidth, imgHeight);
                            context.restore();
                            break;
                        default:
                            break;
                    };
                };

                image.src = blobUrl;
            }
        };

        getInput = (i: number) => {
            return <HTMLInputElement>(document.getElementById('file' + i));
        };

        private fileReady = (i: number) => {
            var input = this.getInput(i);
            return input && input.files && (input.files.length == 1);
        };

        canSubmit = () => {
            return this.authenticated && this.pages().some((i) => { return this.fileReady(i); });
        };

        onSubmit = () => {
            this.uploading = true;
        };

    } // end of class Writing    

    angular.module(app.myAppName, ['ngSanitize', 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Writing', Writing)
    ;

} // end of module


