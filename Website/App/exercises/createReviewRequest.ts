module app.exercises {

    export function showCreateRequestModal($modal: ng.ui.bootstrap.IModalService, exercise: app.IExercise, successCallback?: () => void) {
        app.Modal.openModal($modal,
            '/app/exercises/createReviewRequestModal.html',
            CreateReviewRequestModal,
            {
                exercise: exercise
            },
            (data: app.IReview) => {
                if ((data && data.exerciseId) === exercise.id) {
                    exercise.reviews.push(data);
                    (successCallback || angular.noop)();
                }
            },
            'static',
            'lg'
            );
        ////.opened.then(() => {
        ////    // Set input focus to the price field. But first wait for css animated transitions to complete.
        ////    this.$timeout(() => {
        ////        $('#price1').focus(); // It needs the jQuery, jqLite does not support focus() (does it?) There is document.getElementById(id).focus().
        ////    }, 200);
        ////});
    };

    interface IReviewer { // extends app.IUser 
        id: number;
        displayName: string;
        rate: number;
        selected: boolean;
    }

    class CreateReviewRequestModal extends app.Modal {

        exercise: app.IExercise;
        reviewers: IReviewer[] = [];
        balance: number;
        minPrice: number; // Cache the value to save on calculation of the same expresion in many bindings
        maxPrice: number;
        cutOffIndex: number = 1000;

        constructor(
            $http: ng.IHttpService,
            $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            $scope: app.IScopeWithViewModel,
            modalParams: any
            ) {
            super($http, $modalInstance, $scope, modalParams);
            this.exercise = modalParams.exercise;
            this.getConditions();
        } // ctor

        getConditions = () => {
            app.ngHttpGet(this.$http,
                app.exercisesApiUrl(this.exercise.type + '/review_conditions'),
                null,
                (data) => {
                    this.reviewers = data.reviewers;
                    this.balance = data.balance ? data.balance : 0;
                    // Arbitrary teacher
                    //var anyTeacherId = null; // 3 is hardcoded in dbo.sysInitializeSpecialUsers. The value is used for downloading the avatar.
                    //var anyTeacherDisplayName = 'Any teacher'; // Corresponds to 'Any teacher' in dbo.exeCreateReviewRequest
                    var anyTeacher = <IReviewer>{
                        id: null,
                        displayName: 'Any teacher',
                        rate: data.anyTeacherReviewRate,
                    };
                    this.reviewers.push(anyTeacher);

                    this.reviewers.sort((a, b) => {
                        return (a.rate > b.rate) ? 1 : ((a.rate < b.rate) ? -1 : 0);
                    });

                    // Self-review.
                    var selfUser = app.getSelfUser();
                    var self = <IReviewer>{
                        id: selfUser.id,
                        displayName: selfUser.displayName + ' (yourself)',
                        rate: 0,
                    };
                    this.reviewers.unshift(self);

                    this.reviewers.forEach((i) => { i.selected = null; });
                });
        };

        // Mirrors dbo.CalculateReviewPrice(). Refactor in both places.
        // Length is milliseconds/words. Rate is $/min / $/100words. Minimal price is for 1 minute / 100 words. Round down to full cents.
        private calcPrice = (rate) => {
            var unit;
            switch (this.exercise.type) {
                case app.ExerciseType.AudioRecording:
                    unit = 60000;
                    break;
                case app.ExerciseType.WritingPhoto:
                    unit = 100;
                    break;
                default:
                    unit = undefined;
            };
            var units = this.exercise.length / unit;
            var price = Math.max(units, 1) * rate;
            var cents = Math.floor(price * 100);
            var result = Number((cents / 100).toFixed(2));
            //return [units, price, cents, result];
            return result;
        };

        price = (reviewer: IReviewer) => {
            return this.calcPrice(reviewer.rate);
        };

        onSelected = (r: IReviewer, $index) => {
            if ($index == 0) {
                if (r.selected) {
                    this.reviewers.forEach((i) => {
                        if (i !== r) {
                            i.selected = false;
                        }
                    });
                    this.cutOffIndex = 0;
                }
                else {
                    this.cutOffIndex = 1000;
                }
            }

            this.minPrice = this.getMinPrice();
            this.maxPrice = this.getMaxPrice();
        };

        private minMaxPrice = (fn: (arg1: number, arg2: number) => number, initialValue: number) => {
            var rate = this.reviewers.reduce<number>((prev, cur) => {
                return fn(prev, cur.selected ? cur.rate : prev);
            }, initialValue);
            return rate !== initialValue ? this.calcPrice(rate) : null;
        };

        private getMinPrice = () => {
            return this.minMaxPrice(Math.min, Number.MAX_VALUE);
        };

        private getMaxPrice = () => {
            return this.minMaxPrice(Math.max, -1);
        };

        getAddLink = () => {
            return 'https://' + window.document.location.hostname + '/account/add-money';
        }

        canOk = () => {
            return !this.busy
                && angular.isNumber(this.maxPrice)
                && (this.maxPrice <= this.balance);
        }

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.reviewsApiUrl(),
                {
                    exerciseId: this.exercise.id,
                    reviewers: this.reviewers
                        .filter((i) => { return i.selected; })
                        .map((i) => {
                            return {
                                userId: i.id,
                                price: this.calcPrice(i.rate),
                            }
                        }),
                },
                () => { toastr.success('Review is requested.'); }
                );
        };
    }; // end of class CreateReviewRequestModal

} // end of module app
