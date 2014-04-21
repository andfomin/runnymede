var App;
(function (App) {
    (function (Exercises_Index) {
        var ViewModel = (function () {
            function ViewModel() {
                var _this = this;
                this.realExeObsArr = ko.observableArray([]);
                this.dialogExercise = ko.observable(null);
                this.dialogReview = ko.observable(null);
                this.balance = ko.observable(null);
                this.workDurationRatio = 4;
                this.reward1 = ko.observable(null);
                this.reward2 = ko.observable(null);
                this.isEmpty = ko.observable(false);
                this.isLoading = ko.observable(false);
                this.teachers = ko.observableArray([]);
                this.anyReviewer = ko.observable(true);
                this.selectedTeachers = ko.observableArray([]);
                this.suggestedReward = function (reviewRate) {
                    var exe = _this.dialogExercise();
                    return exe ? App.Utils.numberToMoney(reviewRate * exe.length * _this.workDurationRatio / 3600000) : null;
                };
                this.exercises = this.realExeObsArr.extend({
                    datasource: App.Utils.dataSource(App.Utils.exercisesApiUrl('GetExercises') + App.Utils.getNoCacheUrl(), function (i) {
                        return new App.Model.Exercise(i);
                    }),
                    pager: {
                        limit: 10
                    }
                });

                this.exercises.loading.subscribe(function (value) {
                    // If returned data contains an empty/non-empty array, it works. If the array is null, the subscription gets broken and does not work.
                    _this.isLoading(value);
                    var isEmpty = (_this.realExeObsArr().length === 0) && !value;
                    _this.isEmpty(isEmpty);
                });

                // anyReviewer and selectedTeachers are mutually exclusive.
                this.anyReviewer.subscribe(function (value) {
                    if (value) {
                        _this.selectedTeachers([]);
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
                    $('#cancelRequestDialog').modal();
                };

                this.cancelRequestCmd = ko.asyncCommand({
                    execute: function (complete) {
                        App.Utils.ajaxRequest('delete', App.Utils.reviewsApiUrl(_this.dialogReview().id.toString())).done(function (data) {
                            _this.dialogReview().cancelTime(App.Utils.formatDate(data.cancelTime));
                            toastr.success('Review request canceled.');
                        }).fail(function () {
                            toastr.error('Error canceling review request.');
                            window.setTimeout(function () {
                                _this.exercises.refresh();
                            }, 0);
                        }).always(function () {
                            _this.dialogReview(null);
                            _this.dialogExercise(null);
                            complete();
                            $('#cancelRequestDialog').modal('hide');
                        });
                    },
                    canExecute: function (isExecuting) {
                        return !isExecuting;
                    }
                });

                this.requestReview = function (exercise) {
                    App.Utils.activityIndicator(true);

                    // Request review conditions from the server.
                    App.Utils.ajaxRequest('GET', App.Utils.exercisesApiUrl(exercise.id.toString() + '/ReviewConditions')).done(function (data) {
                        _this.workDurationRatio = data.workDurationRatio;
                        _this.balance(+data.balance);
                        _this.teachers(data.teachers);

                        _this.dialogExercise(exercise);
                        _this.reward1(null);
                        _this.reward2(null);
                        _this.anyReviewer(true);
                        _this.selectedTeachers([]);

                        // Set input focus to the reward field. But first wait for css transitions to complete.
                        $('#createRequestDialog').on('shown.bs.modal', function () {
                            $('#reward1').focus();
                        });

                        $('#createRequestDialog').modal();
                    }).fail(function () {
                        toastr.error('Error getting review conditions.');
                    }).always(function () {
                        App.Utils.activityIndicator(false);
                    });
                };

                this.requestCmd = ko.asyncCommand({
                    execute: function (complete) {
                        App.Utils.ajaxRequest('POST', App.Utils.reviewsApiUrl(), {
                            exerciseId: _this.dialogExercise().id,
                            reward: _this.reward1(),
                            teachers: _this.selectedTeachers()
                        }).done(function (data) {
                            var r = new App.Model.Review(data);
                            _this.dialogExercise().reviews.push(r);
                            toastr.success('Review requested.');
                        }).fail(function (jqXHR) {
                            App.Utils.logAjaxError(jqXHR, 'Review request for exercise failed.');
                        }).always(function () {
                            complete();
                            $('#createRequestDialog').modal('hide');
                            _this.dialogExercise(null);
                            window.setTimeout(function () {
                                _this.exercises.refresh();
                            }, 0);
                        });
                    },
                    canExecute: function (isExecuting) {
                        var rewardStr = _this.reward1();
                        var positive = App.Utils.isValidAmount(rewardStr) && (Number(rewardStr) > 0);
                        var reviewersOk = _this.anyReviewer() || (_this.selectedTeachers().length > 0);
                        return !isExecuting && positive && (_this.reward2() === rewardStr) && reviewersOk;
                    }
                });

                this.submitRequest = function () {
                    _this.requestCmd.execute();
                };

                window.setInterval(function () {
                    _this.exercises.refresh();
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
