module app.account {

    export class PayTeacher {

        email: string;
        amount: string;
        password: string;
        teacherUserId: number;
        teacherDisplayName: string;

        sending: boolean;

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

        init = (teacherUserId, teacherDisplayName) => {
            this.teacherUserId = teacherUserId;
            this.teacherDisplayName = teacherDisplayName;
        };

        isValidAmount = () => {
            return app.isValidAmount(this.amount);
        }

        showConfirmDialog = (form) => {
            if (form.$valid) {
                (<any>$('#confirmDialog')).modal();
            }
        }

            start = () => {


            app.ngHttpPost(this.$http,
                app.accountsApiUrl('payment_to_teacher'),
                {
                    teacherUserId: this.teacherUserId,
                    amount: this.amount,
                    password: this.password
                },
                () => { toastr.success('Payment succeeded.'); },
                () => { (<any>$('#confirmDialog')).modal('hide'); }
                );
        }

    } // end of class

    angular.module(app.myAppName, [])
        .controller('PayTeacher', PayTeacher);

} // end of module

