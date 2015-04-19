module app.ielts {

    export class IeltsBase {
        card: ICard = undefined; // We may receive "null" from the server and we distinguish between these two values in the UI.
        currentItem: number = 0;

        constructor(
            public $http: angular.IHttpService,
            public $modal: angular.ui.bootstrap.IModalService,
            public $scope: app.IScopeWithViewModel
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;

            this.loadCard();
            /* ----- End of constructor  ----- */
        } // end of ctor  

        loadCard = () => {
            app.ngHttpGet(this.$http,
                app.exercisesApiUrl('user_card' + location.pathname),
                null,
                (data) => {
                    this.card = data;
                }
                );
        };

        getSecondaryTitle = () => {
            switch (location.pathname) {
                case '/ielts/writing/1a':
                    return 'Writing Task 1 Academic';
                    break;
                case '/ielts/writing/1g':
                    return 'Writing Task 1 General';
                    break;
                case '/ielts/writing/2a':
                    return 'Writing Task 2 Academic';
                    break;
                case '/ielts/writing/2g':
                    return 'Writing Task 2 General';
                    break;
                case '/ielts/speaking/1':
                    return 'Speaking Part 1';
                    break;
                case '/ielts/speaking/23':
                    return 'Speaking Parts 2 & 3';
                    break;
                default:
                    return '';
            }
        };

        nextItem = () => {
            this.currentItem++;
            if (this.currentItem >= this.card.items.length) {
                this.currentItem = 0;
            }
            app.beep();
        };

        showChooseCardModal = () => {
            this.currentItem = 0;            
            app.exercises.showChooseCardModal(this.$modal,
                (data) => {
                    this.card = data;
                    app.ngHttpPut(this.$http,
                        app.exercisesApiUrl('user_card' + location.pathname + '/' + this.card.id),
                        null
                        );
                });
        };

    } // end of class IeltsBase
} // end of module
