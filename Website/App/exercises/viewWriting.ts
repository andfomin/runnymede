module app.exercises {

    //export class ViewWriting extends app.exercises.ViewCtrlBase {

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

    //        /* ----- End of constructor  ----- */
    //    }

    //} // end of class ViewWriting

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'ngTagsInput'])
        .config(app.library.TagsInputConfig)
        .value(app.ngNames.$appRemarksComparer, app.exercises.WritingsComparer)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .controller('ResourceList', app.library.ResourceList)
        .controller('Canvas', app.exercises.Canvas)
        .controller('ViewWriting', app.exercises.ViewCtrlBase)
    ;

} // end of module

