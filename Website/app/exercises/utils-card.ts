module app.exercises {

    export class CardProvider implements ILazyProvider<ICard> {
        static ServiceName = 'appCardProvider';
        //static SupportedTypes = [ServiceType.IeltsWritingTask1, ServiceType.IeltsWritingTask2, ServiceType.IeltsSpeaking];

        promise: angular.IPromise<ICard>;

        static $inject = [app.ngNames.$http]
        constructor(
            public $http: angular.IHttpService
        ) {
            //this.serviceType = app['serviceTypeParam'];
            //if (IeltsBase.SupportedTypes.indexOf(this.serviceType) != -1) {
            //}
        };

        get = () => {
            if (!this.promise) {
                //var serviceType = app['serviceTypeParam'];
                //var url = app.exercisesApiUrl('user_card/' + serviceType);
                //var url = '/content/aec3cc24-92b4-4cc3-88d1-c235395e30d0.json';
                var cardId = app['cardIdParam'];
                var url = '/app/exercisesIelts/' + cardId + '.json';
                this.promise = app.ngHttpGet(this.$http, url, null, null)
                    .then(
                    (response: angular.IHttpPromiseCallbackArg<ICard>) => {
                        return response.data;
                    },
                    (reason) => { return { message: reason } }
                    );
            }
            return this.promise;
        };

    } // end of class CardProvider

} // end of module app.exercises 