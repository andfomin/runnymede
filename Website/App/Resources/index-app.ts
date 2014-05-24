var app = angular.module('app', ['AppUtilsNg', 'ngRoute', 'chieffancypants.loadingBar']);

app.controller('Collection', App.Resources_Index.Collection);
app.controller('Search', App.Resources_Index.Search);

app.config(['$routeProvider', function ($routeProvider: ng.route.IRouteProvider) {
    var templateUrl = (template: string) => { return '/App/Resources/' + template; };
    $routeProvider
        .when('/search', { templateUrl: templateUrl('index-search.html'), controller: 'Search', title: 'Search' })
        .when('/collection', { templateUrl: templateUrl('index-collection.html'), controller: 'Collection', title: 'Favorite Resources' })
        .otherwise({ redirectTo: '/search' });
}]);

App.Utils.useRouteTitle(app);

