module App.Resources_Index {

    export class Collection {

        sending: boolean;
        url: string;

        static $inject = ['$scope', '$http'];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

        add() {
            this.sending = true;
            App.Utils.ngHttpPost(this.$http,
                App.Utils.resourcesApiUrl(),
                {
                    url: this.url
                },
                () => {
                },
                () => { this.sending = false; });
        }

    } // end of class
} // end of module

