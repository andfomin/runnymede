module app.exercises {

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar', 'vr.directives.slider', 'ngTagsInput'])
        .config(app.library.TagsInputConfig)
        .constant(app.ngNames.$appRemarksComparer, app.exercises.RecordingsComparer)
        .constant(app.ngNames.jQuery, $)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
        .service(app.ngNames.$signalRService, app.SignalRService)
        .controller('ResourceList', app.library.ResourceList)
        .controller('AudioPlayer', app.exercises.AudioPlayer) // vma
        .controller('ViewRecording', app.exercises.ViewCtrlBase)
    ;

} // end of module

