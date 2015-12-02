module app.exercises {

    export function showCreateRequestModal($modal: angular.ui.bootstrap.IModalService, exercise: app.IExercise, successCallback?: () => void) {
        app.Modal.openModal($modal,
            '/app/exercises/createReviewRequestModal.html',
            CreateReviewRequestModal,
            {
                exercise: exercise
            },
            'static'
        )
            .then((data: app.IReview) => {
                if ((data && data.exerciseId) === exercise.id) {
                    if (!angular.isArray(exercise.reviews)) {
                        exercise.reviews = [];
                    }
                    exercise.reviews.push(data);
                    (successCallback || angular.noop)();
                }
            });
    };

    class CreateReviewRequestModal extends app.Modal {

        exercise: app.IExercise;
        balance: number;
        price: number;

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
                app.exercisesApiUrl(this.exercise.id + '/review_conditions'),
                null,
                (data) => {
                    if (data) {
                        this.balance = data.balance || 0;
                        this.price = data.price;
                    }
                });
        };

        showBalance = () => {
            return angular.isNumber(this.balance) && (this.balance < this.price);
        };

        getBuyLink = () => {
            return app.getBuyLink();
        }

        canOk = () => {
            return !this.busy; 
            //&& (this.balance > this.price); // This may be a lucky-you exercise. It can be requested with any balance.
        }

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(),
                {
                    exerciseId: this.exercise.id,
                },
                () => { toastr.success('Thank you for requesting a review'); }
            );
        };
    }; // end of class CreateReviewRequestModal

    export function showChooseCardModal($modal: angular.ui.bootstrap.IModalService, serviceType: string, cardType: string, successCallback: (any) => void) {
        return app.Modal.openModal($modal,
            '/app/exercises/chooseCardModal.html',
            ChooseCardModal,
            {
                serviceType: serviceType,
                cardType: cardType
            },
            true,
            'lg'
        )
            .then(successCallback);
    };

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
            this.getCards(modalParams.serviceType, modalParams.cardType);
        } // ctor

        getCards = (serviceType, cardType) => {
            app.ngHttpGet(this.$http,
                app.exercisesApiUrl('cards/' + serviceType + '/' + (cardType || '_null_')),
                null,
                (data) => { this.cards = data; }
            );
        };

        findOpenedCard = () => {
            return app.arrFind(this.cards, (i) => { return i['open']; });
        };

        canOk = () => {
            return !!this.findOpenedCard();
        };

        internalOk = () => {
            var deferred = this.$q.defer();
            var promise = deferred.promise;
            deferred.resolve(this.findOpenedCard());
            return promise;
        };

    }; // end of class ChooseCardModal

} // end of module app.exercises
