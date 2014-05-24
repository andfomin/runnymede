module App.Resources_Youtube {

    export interface IKeyword {
        value: string;
    }

    export class Add {

        sending: boolean;
        url: string;
        keywords: IKeyword[] = [{ value: null }];

        tag1: string;
        tag2: string;
        tag3: string;

        static $inject = ['$scope', '$http'];

        constructor(
            private $scope: Utils.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            $scope.vm = this;
        } // end of ctor

        keypress(index: number) {
            if (index === this.keywords.length - 1) {
                if (this.keywords[index].value.length > 0)
                    this.keywords.push({ value: null });
            }
        }

        add() {
            this.sending = true;
            App.Utils.ngHttpPost(this.$http,
                App.Utils.resourcesApiUrl(),
                {
                    url : this.url
                },
                () => {
                },
                () => { this.sending = false; });
        }

    } // end of class
} // end of module
