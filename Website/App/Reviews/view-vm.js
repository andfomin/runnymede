var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var App;
(function (App) {
    (function (Reviews) {
        var ViewModel = (function (_super) {
            __extends(ViewModel, _super);
            function ViewModel() {
                var _this = this;
                _super.call(this);

                this.savingSourceName = 'FromView';

                this.saveMapper = function (remarks) {
                    return $.map(remarks, function (i) {
                        return {
                            id: i.id,
                            reviewId: i.reviewId,
                            starred: i.starred
                        };
                    });
                };

                this.saveTitle = function () {
                    if (_this.exercise().isTitleDirty().isDirty()) {
                        var title = $.trim(_this.exercise().title());
                        if (title) {
                            App.Utils.ajaxRequest('PUT', App.Utils.exercisesApiUrl(_this.exercise().id + '/Title'), { title: title }).done(function () {
                                _this.exercise().isTitleDirty().reset();
                                toastr.success('Title saved.');
                            }).fail(function () {
                                toastr.error('Error saving title.');
                            });
                        }
                    }
                };

                this.getRemarkRowClass = function (remark) {
                    var selectedClass = remark === _this.selectedRemark() ? 'selected-remark' : 'not-selected-remark';

                    //var ignoredClass = remark.starred() ? '' : ' ignored-remark';
                    return selectedClass;
                };

                this.internalOnDirty = function () {
                    // Save asynchronously. Calling the command synchronously does not work :(
                    window.setTimeout(function () {
                        _this.saveCmd.execute();
                    }, _this.dirtyAutoSaveInterval);
                };

                this.internalOnBeforeUnload = function (event) {
                    if (_this.exercise().isTitleDirty().isDirty()) {
                        var text = 'If you leave this page without pressing Enter, your changes to the exercise title will be lost.';
                        if (event) {
                            event.returnValue = text;
                        }
                        return text;
                    }
                };
            }
            return ViewModel;
        })(App.Reviews.ViewModelBase);
        Reviews.ViewModel = ViewModel;
    })(App.Reviews || (App.Reviews = {}));
    var Reviews = App.Reviews;
})(App || (App = {}));

$(function () {
    var vm = new App.Reviews.ViewModel();
    ko.applyBindings(vm);
});
//# sourceMappingURL=view-vm.js.map
