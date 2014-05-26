module App.Reviews {

    export class CtrlBase {

        sound: any = null;
        exercise: App.Model.IExercise2;
        review: App.Model.IReview2; // The single review.
        remarks: App.Model.IRemark2[];
        soundPosition: KnockoutObservable<number> = ko.observable(0);
        selectedRemark: KnockoutObservable<App.Model.IRemark> = ko.observable(null);
        saveCmd: KoliteCommand;
        internalOnSaveDone: () => void;
        isDirty: KnockoutComputed<boolean>;
        selectRemark: (remark: App.Model.Remark, event?: any) => void;
        savingSourceName: string;
        saveMapper: (remarks: App.Model.IRemark[]) => any[];
        createSound: () => void;
        onRemarkSelected: (remark: App.Model.IRemark) => void;
        formattedPosition: KnockoutComputed<string>;
        turnPlayer: (play: boolean) => void;
        playerClick: () => void;
        playRemarkSpot: (remark: App.Model.IRemark) => void;
        getRemarkRowClass: (remark: App.Model.IRemark) => void;
        getSpotMarkClass: (remark: App.Model.IRemark) => void;
        internalOnDirty: () => void;
        internalOnBeforeUnload: (event: BeforeUnloadEvent) => string;
        dirtyAutoSaveInterval: number = 0; //msec


        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

    } // end of class
} // end of module
