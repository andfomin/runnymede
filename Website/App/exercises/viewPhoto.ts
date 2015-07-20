module app.exercises {

    angular.module(app.myAppName, [app.utilsNg, 'ngSanitize', 'ui.bootstrap', 'angular-loading-bar', 'ngTagsInput'])
        .config(app.library.TagsInputConfig)
        .constant(app.ngNames.$appRemarksComparer, app.exercises.WritingsComparer)
        .constant(app.ngNames.jQuery, $)
        .service(app.ngNames.$signalRService, app.SignalRService)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .controller('ResourceList', app.library.ResourceList)
        .controller('Canvas', app.exercises.Canvas) // vma
        .controller('ViewPhoto', app.exercises.ViewCtrlBase)
    ;

} // end of module

