module App.Reviews {

    export class ViewModel extends App.Reviews.ViewModelBase {

        saveTitle: () => void;

        constructor() {
            super();

            this.savingSourceName = 'FromView';

            this.saveMapper = (remarks) => {
                return $.map(remarks, (i) => {
                    return {
                        id: i.id,
                        reviewId: i.reviewId,
                        starred: i.starred,
                    };
                });
            };

            this.saveTitle = () => {
                if (this.exercise().isTitleDirty().isDirty()) {
                    var title = $.trim(this.exercise().title());
                    if (title) {
                        App.Utils.ajaxRequest('PUT', App.Utils.exercisesApiUrl(this.exercise().id + '/Title'), { title: title })
                            .done(() => {
                                this.exercise().isTitleDirty().reset();
                                toastr.success('Title saved.');
                            })
                            .fail(() => { toastr.error('Error saving title.'); });
                    }
                }
            };

            this.getRemarkRowClass = (remark) => {
                var selectedClass = remark === this.selectedRemark() ? 'selected-remark' : 'not-selected-remark';
                //var ignoredClass = remark.starred() ? '' : ' ignored-remark';
                return selectedClass;// + ignoredClass;
            };

            this.internalOnDirty = () => {
                // Save asynchronously. Calling the command synchronously does not work :(
                window.setTimeout(() => {
                    this.saveCmd.execute();
                }
                    , this.dirtyAutoSaveInterval);
            };

            this.internalOnBeforeUnload = (event) => {
                if (this.exercise().isTitleDirty().isDirty()) {
                    var text = 'If you leave this page without pressing Enter, your changes to the exercise title will be lost.';
                    if (event) {
                        event.returnValue = text;
                    }
                    return text;
                }
            };

        } // end of ctor

    } // end of class
}

$(() => {
    var vm = new App.Reviews.ViewModel();
    ko.applyBindings(vm);
});

