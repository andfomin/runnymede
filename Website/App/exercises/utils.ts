module app.exercises {

    export function showCreateRequestModal($modal: angular.ui.bootstrap.IModalService, exercise: app.IExercise, successCallback?: () => void) {
        app.Modal.openModal($modal,
            '/app/exercises/createReviewRequestModal.html',
            CreateReviewRequestModal,
            {
                exercise: exercise
            },
            (data: app.IReview) => {
                if ((data && data.exerciseId) === exercise.id) {
                    if (!angular.isArray(exercise.reviews)) {
                        exercise.reviews = [];
                    }
                    exercise.reviews.push(data);
                    (successCallback || angular.noop)();
                }
            },
            'static'
            );
    };

    export function showChooseCardModal($modal: angular.ui.bootstrap.IModalService, successCallback: (any) => void) {
        app.Modal.openModal($modal,
            '/app/exercises/chooseCardModal.html',
            ChooseCardModal,
            null,
            successCallback,
            true,
            'lg'
            );
    };

    class CreateReviewRequestModal extends app.Modal {

        exercise: app.IExercise;
        price: number;
        balance: number;

        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.exercise = modalParams.exercise;
            this.getConditions();
        } // ctor

        getConditions = () => {
            app.ngHttpGet(this.$http,
                app.exercisesApiUrl('review_conditions'),
                {
                    exerciseType: this.exercise.type,
                    length: this.exercise.length,
                },
                (data) => {
                    this.price = data.price;
                    this.balance = data.balance ? data.balance : 0;
                });
        };

        getBuyLink = () => {
            return 'https://' + window.document.location.hostname + '/account/buy-reviews';
        }

        canOk = () => {
            return !this.busy
                && angular.isNumber(this.price)
                && angular.isNumber(this.balance)
                && (this.price <= this.balance);
        }

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(),
                {
                    exerciseId: this.exercise.id,
                    price: this.price,
                },
                () => { toastr.success('Thank you for requesting review'); }
                );
        };
    }; // end of class CreateReviewRequestModal

    class ChooseCardModal extends app.Modal {

        cards: ICard[];

        static $inject = [app.ngNames.$http, app.ngNames.$modalInstance, app.ngNames.$q, app.ngNames.$scope, 'modalParams'];

        constructor(
            $http: angular.IHttpService,
            $modalInstance: angular.ui.bootstrap.IModalServiceInstance,
            private $q: angular.IQService,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.getCards();
        } // ctor

        getCards = () => {
            app.ngHttpGet(this.$http,
                app.exercisesApiUrl('cards' + location.pathname),
                null,
                (data) => {
                    this.cards = data;
                }
                );
        };

        findOpenCard = () => {
            return app.arrFind(this.cards,(i) => { return i['open']; });
        };

        canOk = () => {
            return !!this.findOpenCard();
        };

        internalOk = () => {
            var deferred = this.$q.defer();
            var promise = deferred.promise;
            deferred.resolve(this.findOpenCard());
            return promise;
        };

    }; // end of class ChooseCardModal


} // end of module exercises
