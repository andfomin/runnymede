module App.Reviews_Index {

    export class ViewModel {

        reviews: KnockoutObservableArray<App.Model.IReview>;

        constructor() {

            this.reviews = ko.observableArray([]).extend({
                datasource: App.Utils.dataSource(
                    App.Utils.reviewsApiUrl() + App.Utils.getNoCacheUrl(),
                    i => { return new App.Model.Review(i); }
                    ),
                pager: {
                    limit: 10
                }
            });
        } // end of ctor

    } // end of class
}

$(() => {
    var vm = new App.Reviews_Index.ViewModel();
    ko.applyBindings(vm);
});
