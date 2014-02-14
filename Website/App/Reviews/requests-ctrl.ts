module App.Reviews_Requests {

    export interface IRequest {
        id: number;
        reviewId: number;
        reward: number;
        authorName: string;
        typeId: string;
        length: string;
    }

    export class Ctrl {

        requests: IRequest[] = [];
        dialogRequest: IRequest;

        static $inject = [App.Utils.AngularGlobal.$SCOPE, App.Utils.AngularGlobal.$HTTP];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;

            this.refresh();
            window.setInterval(this.refresh, 30000);
        } // end of ctor

        private refresh = () => {
            App.Utils.ngHttpGet(this.$http,
                App.Utils.reviewsApiUrl('Requests'),
                (data) => { this.requests = data; }
                );
        }

        showStartDialog = (request: IRequest) => {
            this.dialogRequest = request;
            (<any>$('#startDialog')).modal();
        }

        start = () => {
            var request = this.dialogRequest;
            this.dialogRequest = null;

            if (request) {
                App.Utils.ngHttpPost(this.$http,
                    App.Utils.reviewsApiUrl(request.reviewId.toString() + '/Start'),
                    null,
                    () => { window.location.assign(App.Utils.reviewsUrl('edit/' + request.reviewId.toString())); },
                    () => { (<any>$('#startDialog')).modal('hide'); }
                    );
            }
        }

        formatMsec = (length: number) => {
            return App.Utils.formatMsec(length);
        }

    } // end of class
} // end of module

var app = angular.module('app', []);
app.controller('Ctrl', App.Reviews_Requests.Ctrl);



