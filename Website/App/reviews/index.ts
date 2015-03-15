module app.reviews {

    function getEditUrl(id: number) {
        return app.reviewsUrl('edit/' + id);
    }

    export class Requests extends app.CtrlBase {

        unfinishedReviewId: number = null;
        requests: IReview[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$interval, app.ngNames.$window];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $modal: angular.ui.bootstrap.IModalService,
            $interval: angular.IIntervalService,
            private $window: angular.IWindowService
            ) {
            super($scope);
            this.getRequests();
            var interval = $interval(() => { this.getRequests(); }, 60000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });
        } // end of ctor
        
        private getRequests = () => {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.reviewsApiUrl('requests'),
                    null,
                    (data) => {
                        this.requests = data || [];
                        // If the user has an unfinished review, do not show them requests. We return the review with exerciseType set to 'unfinished' to indicate that case.
                        this.unfinishedReviewId = (this.requests.length === 1) && (this.requests[0].exerciseType == 'unfinished' ? this.requests[0].id : null);
                    });
            }
        }

        unfinishedUrl = () => {
            return this.unfinishedReviewId ? getEditUrl(this.unfinishedReviewId) : null;
        };

        showStartReviewModal = (request) => {
            app.Modal.openModal(this.$modal,
                '/app/reviews/startReviewModal.html',
                StartReviewModal,
                {
                    request: request
                },
                () => {
                    this.$window.location.assign(getEditUrl(request.id));
                }
                );
        };

    } // end of class Requests

    export class Reviews extends app.CtrlBase {

        reviews: app.IReview[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$interval];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            $interval: angular.IIntervalService
            ) {
            super($scope);
            this.pgLoad();
            var interval = $interval(() => { this.pgLoad(); }, 300000);
            $scope.$on('$destroy',() => { $interval.cancel(interval); });
        } // end of ctor

        pgLoad() {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.reviewsApiUrl(),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit
                    },
                    (data) => {
                        this.reviews = data.items;
                        this.pgTotal = data.totalCount;
                    }
                    );
            }
        }

        isEmpty() {
            return this.reviews && (this.reviews.length == 0);
        }

        getEditUrl(r: app.IReview) {
            return getEditUrl(r.id);
        }

    } // end of class Index_Reviews

    export class StartReviewModal extends app.Modal {

        request: IReview;

        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.request = modalParams.request;
        } // ctor

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(this.request.id.toString() + '/start'),
                null
                );
        };
    }

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Requests', Requests)
        .controller('Reviews', Reviews);

} // end of module app.reviews

