module app.storytelling {

    interface IRootCategory {
        id: string;
        title: string;
    }

    interface IStoryVideo {
        id: string; // GUID
        rootCategory: string;
        blobName: string;
        description: string;
    }

    export class Speak {

        rootCategories: IRootCategory[];
        video: IStoryVideo;

        static $inject = [app.ngNames.$http, app.ngNames.$uibModal, app.ngNames.$scope, app.ngNames.$window];
        constructor(
            private $http: angular.IHttpService,
            private $modal: angular.ui.bootstrap.IModalService,
            private $scope: app.IScopeWithViewModel,
            private $window: angular.IWindowService
        ) {
            $scope.vm = this;

        } // end of ctor

        newStory = () => {
            app.Modal.openModal(this.$modal,
                'chooseRootCategory.html',
                ChooseRootCategoryModal,
                null,
                true,
                'sm'
            )
                .then((category: IRootCategory) => {
                    app.ngHttpGet(this.$http,
                        app.storytellingApiUrl('blobName/' + category.id),
                        null,
                        (data) => {
                            if (data) {
                                this.video = data;
                                var url = app.getBlobUrl('storytelling-videos', this.video.blobName);
                                // The videos are small, so we can cache them in the local memory after a single request.
                                // This way we avoid multiple "Range: bytes" requests by the video player.
                                this.$http.get<Blob>(url, { responseType: "blob", })
                                    .then(
                                    (response) => {
                                        var player = <HTMLVideoElement>this.$window.document.getElementById('myVideo');
                                        player.src = URL.createObjectURL(response.data);
                                        //player.play();
                                    },
                                    app.logError
                                    );
                            }
                        }
                    );
                });
        };


    } // end of class Speak

    export class ChooseRootCategoryModal extends app.Modal {

        // Root categories correspond to those at +http://www.ispot.tv/browse
        rootCategories: IRootCategory[] = [
            { id: 'k', title: 'Apparel, Footwear & Accessories' },
            { id: 'Y', title: 'Business & Legal' },
            { id: '7', title: 'Education' },
            { id: 'A', title: 'Electronics & Communication' },
            { id: 'd', title: 'Food & Beverage' },
            { id: 'I', title: 'Health & Beauty' },
            { id: 'o', title: 'Home & Real Estate' },
            { id: 'Z', title: 'Insurance' },
            { id: 'w', title: 'Life & Entertainment' },
            { id: '7k', title: 'Pharmaceutical & Medical' },
            { id: 'q', title: 'Politics, Government & Organizations' },
            { id: 'b', title: 'Restaurants' },
            { id: '2', title: 'Retail Stores' },
            { id: '5', title: 'Travel' },
            { id: 'L', title: 'Vehicles' },
        ];

        static $inject = [app.ngNames.$http, app.ngNames.$modalInstance, app.ngNames.$q, app.ngNames.$scope, 'modalParams'];
        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            private $q: angular.IQService,
            $scope: app.IScopeWithViewModel,
            modalParams: any
        ) {
            super($http, $modalInstance, $scope, modalParams);

        } // ctor

        canOk = () => {
            return !this.busy;
        };

        internalOk = (category: IRootCategory) => {
            var deferred = this.$q.defer<IRootCategory>();
            deferred.resolve(category);
            return deferred.promise;
        };

    }; // end of class ChooseRootCategoryModal

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Speak', Speak);

} // end of module app.storytelling