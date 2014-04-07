module App.Account_PayMoney {

    export class Ctrl {

        email: string;
        amount: string;
        password: string;
        teacherUserId: number;
        teacherDisplayName: string;

        sending: boolean;

        static $inject = [App.Utils.ngNames.$scope, App.Utils.ngNames.$http];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;

            (<any>$scope).init = (teacherUserId, teacherDisplayName) => {
                this.teacherUserId = teacherUserId;
                this.teacherDisplayName = teacherDisplayName;
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
                App.Utils.accountApiUrl('PaymentToTeacher'),
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
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Account_PayMoney.Ctrl);
