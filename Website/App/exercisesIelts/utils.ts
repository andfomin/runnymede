module app.exercisesIelts {

    export class IeltsBase {

        authenticated: boolean;
        serviceType: string;
        cardType: string;
        card: ICard = undefined; // We may receive "null" from the server and we distinguish between these two values in the UI.

        constructor(
            public $http: angular.IHttpService,
            public $modal: angular.ui.bootstrap.IModalService,
            public $scope: app.IScopeWithViewModel
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;
            this.authenticated = app.isAuthenticated();
            this.serviceType = app['serviceTypeParam'];

            if ([ServiceType.IeltsWritingTask1, ServiceType.IeltsWritingTask2, ServiceType.IeltsSpeaking].indexOf(this.serviceType) != -1) {
                this.loadCard();
            }
            /* ----- End of constructor  ----- */
        } // end of ctor  

        loadCard = () => {
            if (this.authenticated) {
                app.ngHttpGet(this.$http,
                    app.exercisesApiUrl('user_card/' + this.serviceType),
                    null,
                    (data) => {
                        if (data) {
                            this.card = data;
                            this.cardType = this.card && this.card.type;
                        }
                    }
                    );
            }
        };

        resetCard = () => { }; // overriden in descendants.

        showChooseCardModal = () => {
            this.resetCard();
            app.exercises.showChooseCardModal(this.$modal, this.serviceType, this.cardType,
                (data) => {
                    this.card = data;
                    if (this.authenticated) {
                        app.ngHttpPut(this.$http,
                            app.exercisesApiUrl('user_card/' + this.serviceType),
                            {
                                id: this.card.id
                            }
                            );
                    }
                });
        };

    } // end of class IeltsBase

} // end of module
