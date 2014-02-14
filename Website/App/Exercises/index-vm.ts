module App.Exercises_Index {

    export class ViewModel {

        realExeObsArr: KnockoutObservableArray<App.Model.Exercise> = ko.observableArray([]); // this.exercises is a wrapper around observableArray and does not expose its methods.
        exercises: KnockoutObservableArray<App.Model.Exercise>;
        deleteExercise: (exercise: App.Model.Exercise) => void;
        requestReview: (exercise: App.Model.Exercise) => void;
        cancelRequest: (exercise: App.Model.Review) => void;
        requestCmd: KoliteCommand;
        deleteExerciseCmd: KoliteCommand;
        cancelRequestCmd: KoliteCommand;
        dialogExercise: KnockoutObservable<App.Model.Exercise> = ko.observable();
        dialogReview: KnockoutObservable<App.Model.Review> = ko.observable();
        balance: KnockoutObservable<number> = ko.observable();
        workDurationRatio: number = 4; // Average ratio of work duration to exercise length. It is used for calculation of suggested offers.
        reward1: KnockoutObservable<string> = ko.observable();
        reward2: KnockoutObservable<string> = ko.observable();
        submitRequest: () => void;
        isEmpty: KnockoutObservable<boolean> = ko.observable(false);

        isLoading: KnockoutObservable<boolean> = ko.observable(false);
        reward: KnockoutComputed<number>;
        tutors: KnockoutObservableArray<App.Model.IUser> = ko.observableArray([]);
        anyReviewer: KnockoutObservable<boolean> = ko.observable(true);
        selectedTutors: KnockoutObservableArray<number> = ko.observableArray([]);

        constructor() {

            this.exercises = this.realExeObsArr.extend({
                datasource: App.Utils.dataSource(
                    // Modern browsers aggresively cache the page.
                    App.Utils.exercisesApiUrl('GetExercises') + App.Utils.getNoCacheUrl(),
                    (i) => { return new Model.Exercise(i); }
                    ),
                pager: {
                    limit: 10
                }
            });

            (<any>this.exercises).loading.subscribe((value) => {
                // If returned data contains an empty/non-empty array, it works. If the array is null, the subscription gets broken and does not work.
                this.isLoading(value);
                var isEmpty = (this.realExeObsArr().length === 0) && !value;
                this.isEmpty(isEmpty);
            });

            // anyReviewer and selectedTutors are mutually exclusive.
            this.anyReviewer.subscribe((value) => {
                if (value) {
                    this.selectedTutors([]);
                }
            });

            this.reward = ko.computed(() => {
                var str = this.reward1();
                var valid = App.Utils.isValidAmount(str);
                var r = valid ? +(str.replace(',', '.')) : null;
                return r;
            });

            this.cancelRequest = (review) => {
                this.dialogReview(review);
                var exercise = App.Utils.find(this.exercises(),
                    (el: App.Model.Exercise) => {
                        return el.reviews().some((r) => r === review);
                    });
                this.dialogExercise(exercise);
                (<any>$('#cancelRequestDialog')).modal();
            };

            this.cancelRequestCmd = ko.asyncCommand({
                execute: (complete) => {
                    App.Utils.ajaxRequest('delete', App.Utils.reviewsApiUrl(this.dialogReview().id.toString()))
                        .done((data) => {
                            this.dialogReview().cancelTime(App.Utils.formatDate(data.cancelTime));
                            toastr.success('Review request canceled.');
                        })
                        .fail(() => {
                            toastr.error('Error canceling review request.');
                            window.setTimeout(() => { (<any>this.exercises).refresh(); }, 0);
                        })
                        .always(() => {
                            this.dialogReview(null);
                            this.dialogExercise(null);
                            complete();
                            (<any>$('#cancelRequestDialog')).modal('hide');
                        });
                },
                canExecute: (isExecuting) => {
                    return !isExecuting;
                }
            });

            this.requestReview = (exercise) => {
                App.Utils.activityIndicator(true);

                // Request review conditions from the server.
                App.Utils.ajaxRequest('GET', App.Utils.exercisesApiUrl(exercise.id.toString() + '/ReviewConditions'))
                    .done((data) => {
                        this.workDurationRatio = data.workDurationRatio;
                        this.balance(+data.balance);
                        this.tutors(data.tutors);

                        this.dialogExercise(exercise);
                        this.reward1(null);
                        this.reward2(null);
                        this.anyReviewer(true);
                        this.selectedTutors([]);

                        // Set input focus to the reward field. But first wait for css transitions to complete.
                        $('#createRequestDialog').on('shown.bs.modal', function () {
                            $('#reward1').focus();
                        });

                        (<any>$('#createRequestDialog')).modal();
                    })
                    .fail(() => {
                        toastr.error('Error getting review conditions.');
                    })
                    .always(function () {
                        App.Utils.activityIndicator(false);
                    });
            };

            this.requestCmd = ko.asyncCommand({
                execute: (complete) => {
                    App.Utils.ajaxRequest('POST',
                        App.Utils.reviewsApiUrl(),
                        {
                            exerciseId: this.dialogExercise().id,
                            reward: this.reward1(),
                            tutors: this.selectedTutors(),
                        })
                        .done((data) => {
                            var r = new App.Model.Review(data);
                            this.dialogExercise().reviews.push(r);
                            toastr.success('Review requested.');
                        })
                        .fail((jqXHR: any) => {
                            App.Utils.logAjaxError(jqXHR, 'Review request for exercise failed.');
                        })
                        .always(() => {
                            complete();
                            (<any>$('#createRequestDialog')).modal('hide');
                            this.dialogExercise(null);
                            window.setTimeout(() => { (<any>this.exercises).refresh(); }, 0);
                        });
                },
                canExecute: (isExecuting) => {
                    var rewardStr = this.reward1();
                    var positive = App.Utils.isValidAmount(rewardStr) && (Number(rewardStr) > 0);
                    var reviewersOk = this.anyReviewer() || (this.selectedTutors().length > 0);
                    return !isExecuting
                        && positive
                        && (this.reward2() === rewardStr)
                        && reviewersOk;
                }
            });

            this.submitRequest = () => {
                this.requestCmd.execute();
            }

            window.setInterval(() => {
                (<any>this.exercises).refresh();
            }
                , 300000);


        } // end of ctor

        suggestedReward = (rate: number) => {
            var exe = this.dialogExercise();
            return exe
                ? App.Utils.numberToMoney(rate * exe.length * this.workDurationRatio / 3600000) // MSec to Hour
                : null;
        };

    } // end of class
}

$(() => {
    var vm = new App.Exercises_Index.ViewModel();
    ko.applyBindings(vm);
});
