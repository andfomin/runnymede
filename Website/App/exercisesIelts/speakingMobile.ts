module app.exercisesIelts {

    export class SpeakingMobile extends app.exercisesIelts.IeltsBase {

        next: number = 0;

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

            // this.supported = app.isMobileDevice() && captureSupported('audio/*');
            // ng-change does not support input["file"]. We attach a handler to the native event on the element.
            (<any>SpeakingMobile).onFileChange = this.onFileChange;


            /* ----- End of constructor  ----- */
        } // end of ctor  

        onFileChange = (event: Event, id: string) => {
            var input = <HTMLInputElement>event.target;
            if (input.files.length > 1) {
                toastr.warning('Only one file per placeholder please!');
            }

            console.log(id);

            this.next = 1 + (+id.substring(4));            

            this.$timeout(() => {
                if (!this.$scope.$$phase)
                    this.$scope.$apply();
            }, 100);
        };

        isActive = (i: number) => {
            return this.card && (i == this.next);
        };

        canSave = () => {
            return app.isAuthenticated() && this.next;
        };


    } // end of class Ctrl    

    //angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ui.router'])
    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ngSanitize'])
        .controller('SpeakingMobile', SpeakingMobile)
    ;

} // end of module
