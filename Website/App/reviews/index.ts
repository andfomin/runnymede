module app.reviews {

    export interface IRequest {
        reviewId: number;
        reviewerUserId: number;
        price: number;
        exerciseType: string;
        exerciseLength: number;
        authorUserId: number;
        authorName: string;
    }

    function getEditUrl(id: number) {
        return app.reviewsUrl('edit/' + id);
    }

    export class Index_Requests extends app.CtrlBase {

        unfinishedReviewId: number = null;
        requests: IRequest[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$modal, app.ngNames.$interval, app.ngNames.$window];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $modal: ng.ui.bootstrap.IModalService,
            $interval: ng.IIntervalService,
            private $window: ng.IWindowService
            ) {
            super($scope);
            this.getRequests();
            var interval = $interval(() => { this.getRequests(); }, 60000);
            $scope.$on('$destroy', () => { $interval.cancel(interval); });
        } // end of ctor
        
        private getRequests = () => {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.reviewsApiUrl('requests'),
                    null,
                    (data) => {
                        this.requests = data.items || [];
                        // If the user has an unfinished review, do not show them requests. We return the ReviewId without a Price to indicate that case (Price is not nullable.)
                        this.unfinishedReviewId =
                        ((this.requests.length === 1) && (!angular.isDefined(this.requests[0].price)))
                        ? this.requests[0].reviewId
                        : null;
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
                    this.$window.location.assign(getEditUrl(request.reviewId));
                }
                );
        };

    } // end of class Index_Requests

    export class Index_Reviews extends app.CtrlBase {

        reviews: app.IReview[] = null;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$interval];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            $interval: ng.IIntervalService
            ) {
            super($scope);
            this.pgLoad();
            var interval = $interval(() => { this.pgLoad(); }, 300000);
            $scope.$on('$destroy', () => { $interval.cancel(interval); });
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

        request: IRequest;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.request = modalParams.request;
        } // ctor

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(this.request.reviewId.toString() + '/start'),
                null,
                null,
                null
                );
        };
    }

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Index_Requests', Index_Requests)
        .controller('Index_Reviews', Index_Reviews);

} // end of module app.reviews

