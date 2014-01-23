var App;
(function (App) {
    (function (Exercises_Index) {
        var ViewModel = (function () {
            //calcRate: (rateARec: number, dialogExercise: KnockoutObservable<App.Model.Exercise>) => string;
            function ViewModel() {
                var _this = this;
                this.realExeObsArr = ko.observableArray([]);
                this.dialogExercise = ko.observable();
                this.dialogReview = ko.observable();
                this.balance = ko.observable();
                this.reward1 = ko.observable();
                this.reward2 = ko.observable();
                this.isEmpty = ko.observable(false);
                this.isLoading = ko.observable(false);
                this.tutors = ko.observableArray([]);
                this.anyReviewer = ko.observable(true);
                this.selectedTutors = ko.observableArray([]);
                this.calcRate = function (rateARec) {
                    var exe = _this.dialogExercise();
                    return exe ? App.Utils.numberToMoney(rateARec * exe.length / 60000) : null;
                };
                this.exercises = this.realExeObsArr.extend({
                    datasource: App.Utils.dataSource(App.Utils.exercisesApiUrl('GetExercises') + App.Utils.getNoCacheUrl(), function (i) {
                        return new App.Model.Exercise(i);
                    }),
                    pager: {
                        limit: 10
                    }
                });

                (this.exercises).loading.subscribe(function (value) {
                    // If returned data contains an empty/non-empty array, it works. If the array is null, the subscription gets broken and does not work.
                    _this.isLoading(value);
                    var isEmpty = (_this.realExeObsArr().length === 0) && !value;
                    _this.isEmpty(isEmpty);
                });

                // anyReviewer and selectedTutors are mutually exclusive.
                this.anyReviewer.subscribe(function (value) {
                    if (value) {
                        _this.selectedTutors([]);
                    }
                });

                this.reward = ko.computed(function () {
                    var str = _this.reward1();
                    var valid = App.Utils.isValidAmount(str);
                    var r = valid ? +(str.replace(',', '.')) : null;
                    return r;
                });

                this.cancelRequest = function (review) {
                    _this.dialogReview(review);
                    var exercise = App.Utils.find(_this.exercises(), function (el) {
                        return el.reviews().some(function (r) {
                            return r === review;
                        });
                    });
                    _this.dialogExercise(exercise);
                    ($('#cancelRequestDialog')).modal();
                };

                this.cancelRequestCmd = ko.asyncCommand({
                    execute: function (complete) {
                        App.Utils.ajaxRequest('delete', App.Utils.reviewsApiUrl(_this.dialogReview().id.toString())).done(function (data) {
                            _this.dialogReview().cancelTime(App.Utils.formatDate(data.cancelTime));
                            toastr.success('Review request canceled.');
                        }).fail(function () {
                            toastr.error('Error canceling review request.');
                            window.setTimeout(function () {
                                (_this.exercises).refresh();
                            }, 0);
                        }).always(function () {
                            _this.dialogReview(null);
                            _this.dialogExercise(null);
                            complete();
                            ($('#cancelRequestDialog')).modal('hide');
                        });
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting;
                    }
                });

                this.requestReview = function (exercise) {
                    // Show an NETEYE Activity Indicator.
                    ($(document.body)).activity();

                    // Request review conditions from the server.
                    App.Utils.ajaxRequest('GET', App.Utils.exercisesApiUrl(exercise.id.toString() + '/ReviewConditions')).done(function (data) {
                        _this.balance(+data.balance);
                        _this.tutors(data.tutors);

                        _this.dialogExercise(exercise);
                        _this.reward1(null);
                        _this.reward2(null);
                        _this.anyReviewer(true);
                        _this.selectedTutors([]);

                        // Set input focus to the reward field. But first wait for css transitions to complete.
                        $('#createRequestDialog').on('shown.bs.modal', function () {
                            $('#reward1').focus();
                        });

                        ($('#createRequestDialog')).modal();
                    }).fail(function () {
                        toastr.error('Error getting review conditions.');
                    }).always(function () {
                        ($(document.body)).activity(false);
                    });
                };

                this.requestCmd = ko.asyncCommand({
                    execute: function (complete) {
                        App.Utils.ajaxRequest('POST', App.Utils.reviewsApiUrl(), {
                            exerciseId: _this.dialogExercise().id,
                            reward: _this.reward1(),
                            tutors: _this.selectedTutors()
                        }).done(function (data) {
                            var r = new App.Model.Review(data);
                            _this.dialogExercise().reviews.push(r);
                            toastr.success('Review requested.');
                        }).fail(function () {
                            toastr.error('Review request for exercise failed.');
                        }).always(function () {
                            complete();
                            ($('#createRequestDialog')).modal('hide');
                            _this.dialogExercise(null);
                            window.setTimeout(function () {
                                (_this.exercises).refresh();
                            }, 0);
                        });
                    },
                    canExecute: function (isExecuting) {
                        var rewardStr = _this.reward1();
                        var valid = App.Utils.isValidAmount(rewardStr);
                        var reviewersOk = _this.anyReviewer() || (_this.selectedTutors().length > 0);
                        return !isExecuting && valid && (_this.reward2() === rewardStr) && reviewersOk;
                    }
                });

                this.submitRequest = function () {
                    _this.requestCmd.execute();
                };

                window.setInterval(function () {
                    (_this.exercises).refresh();
                }, 300000);
            }
            return ViewModel;
        })();
        Exercises_Index.ViewModel = ViewModel;
    })(App.Exercises_Index || (App.Exercises_Index = {}));
    var Exercises_Index = App.Exercises_Index;
})(App || (App = {}));

$(function () {
    var vm = new App.Exercises_Index.ViewModel();
    ko.applyBindings(vm);
});
//# sourceMappingURL=index-vm.js.map
