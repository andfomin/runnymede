module App.Account_PayMoney {

    export class Ctrl {

        email: string;
        amount: string;
        password: string;
        tutorUserId: number;
        tutorDisplayName: string;

        sending: boolean;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;

            (<any>$scope).init = (tutorUserId, tutorDisplayName) => {
                this.tutorUserId = tutorUserId;
                this.tutorDisplayName = tutorDisplayName;
            };
        } // end of ctor

        isValidAmount = () => {
            return App.Utils.isValidAmount(this.amount);
        }

        showConfirmDialog = (form) => {
            if (form.$valid) {
                (<any>$('#confirmDialog')).modal();
            }
        }

            start = () => {


            App.Utils.ngHttpPost(this.$http,
                App.Utils.accountApiUrl('PaymentToTutor'),
                {
                    tutorUserId: this.tutorUserId,
                    amount: this.amount,
                    password: this.password
                },
                () => { toastr.success('Payment succeeded.'); },
                () => { (<any>$('#confirmDialog')).modal('hide'); }
                );
        }




    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Account_PayMoney.Ctrl);
