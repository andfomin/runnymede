module app.exercises {

    //export function isMobileDevice() {
    //    var ver = window.navigator.appVersion;
    //    var ua = window.navigator.userAgent.toLowerCase();
    //    var mobile = (ver.indexOf("iPad") != -1)
    //        || (ver.indexOf("iPhone") != -1)
    //        || (ua.indexOf("android") != -1)
    //        || (ua.indexOf("ipod") != -1)
    //        || (ua.indexOf("windows ce") != -1)
    //        || (ua.indexOf("windows phone") != -1);
    //    return !!mobile;
    //}

    export function captureSupported(accept: string) {
        // element will be garbage-collected on function return.
        //var element = (<Document>(<any>this.$document[0])).createElement('input'); 
        var element = document.createElement('input');
        element.setAttribute('type', 'file');
        element.setAttribute('accept', accept);
        /* Working Draft +http://www.w3.org/TR/2012/WD-html-media-capture-20120712/ described string values for the capture attribute.
         * Candidate Recommendation +http://www.w3.org/TR/2013/CR-html-media-capture-20130509/ specifies that the capture attribute is of type boolean.
         */
        element.setAttribute('capture', <any>true);
        return element.hasAttribute('accept') && element.hasAttribute('capture');
    }

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

        getAddLink = () => {
            return 'https://' + window.document.location.hostname + '/account/add-money';
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

} // end of module exercises
