module app.exercises {

    angular.module(app.myAppName, [app.utilsNg, 'ngSanitize', 'ui.bootstrap', 'angular-loading-bar', 'vr.directives.slider', 'ngTagsInput'])
        .constant(app.ngNames.$appRemarksComparer, app.exercises.RecordingsComparer)
        .constant(app.ngNames.jQuery, $)
        .config(app.library.TagsInputConfig)
        .config(app.exercises.artifactPlayerProviderConfig)
        .provider(app.exercises.PlayerProvider.ServiceName, app.exercises.PlayerProvider)
        .service(app.exercises.CardProvider.ServiceName, app.exercises.CardProvider)
        .service(app.ngNames.$appRemarks, app.exercises.RemarksService)
    //.service(app.ngNames.$signalRService, app.SignalRService) // Something goes wrong on initialization. Test step-by-step while enabling back.
        .controller('ResourceList', app.library.ResourceList)
        .controller('AudioPlayer', app.exercises.AudioPlayer) // vma
        .controller('ViewRecording', app.exercises.ViewCtrlBase)
    ;

} // end of module

