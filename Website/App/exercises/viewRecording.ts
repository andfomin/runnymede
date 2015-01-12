module app.exercises {

    //export class ViewRecording extends app.exercises.ViewCtrlBase {

    //    static $inject = [app.ngNames.$appRemarks, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$rootScope, app.ngNames.$scope];

    //    constructor(
    //        $appRemarks: app.exercises.IRemarksService,
    //        $http: ng.IHttpService,
    //        $modal: ng.ui.bootstrap.IModalService,
    //        $rootScope: ng.IRootScopeService,
    //        $scope: app.IScopeWithViewModel
    //        ) {
    //        /* ----- Constructor  ----- */
    //        super($appRemarks, $http, $modal, $rootScope, $scope);
    //        /* ----- End of ctor  ----- */
    //    }

    //} // end of class ViewRecording

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'vr.directives.slider', 'ngTagsInput'])
        .config(app.library.TagsInputConfig)
        .value(app.ngNames.$appRemarksComparer, app.exercises.RecordingsComparer)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .controller('ResourceList', app.library.ResourceList)
        .controller('AudioPlayer', app.exercises.AudioPlayer)
        .controller('ViewRecording', app.exercises.ViewCtrlBase)
    ;

} // end of module

