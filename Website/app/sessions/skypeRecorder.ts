module app.sessions {

    export class SkypeRecorder {

        uuid: string; // The Uuid of the Skype call assigned by Freeswitch
        callStarted: Date;
        started: Date;
        stopped: Date;
        spots: Date[];
        spotsText: string;
        learnerSkype: string = 'ssssssssskkkkkyyyyppeee';
        title: string;
        comment: string;
        busy: boolean;

        static $inject = [app.ngNames.$http, app.ngNames.$scope, app.ngNames.$timeout, app.ngNames.$window];

        constructor(
            private $http: angular.IHttpService,
            private $scope: app.IScopeWithViewModel,
            private $timeout: angular.ITimeoutService,
            private $window: angular.IWindowService
            ) {
            $scope.vm = this;
        } // end of ctor

        start = () => {
            this.busy = true;
            app.ngHttpPut(this.$http,
                app.skypeRecordingsApiUrl(),
                {
                    learnerSkype: this.learnerSkype,
                },
                (data) => {
                    this.uuid = data;
                    this.started = new Date();
                    this.stopped = null;
                    this.spots = [];
                    this.$timeout(() => {
                        var input = document.getElementById('my-spots');
                        input.focus();
                    }, 200);
                },
                () => { this.busy = false; }
                );
        };

        stop = () => {
            this.busy = true;
            this.stopped = new Date();
            app.ngHttpPost(this.$http,
                app.skypeRecordingsApiUrl(this.uuid + '/stop'),
                null,
                null,
                () => { this.busy = false; }
                );
        };

        addSpot = () => {
            this.spots.push(new Date());
        };

        canSave = () => {
            return this.started && this.stopped && this.spots && this.spots.length && !this.busy;
        };

        save = () => {
            this.busy = true;
            var started = this.started.getTime();
            app.ngHttpPut(this.$http,
                app.skypeRecordingsApiUrl(this.uuid + '/save'),
                {
                    title: this.title,
                    learnerSkype: this.learnerSkype,
                    comment: this.comment,
                    remarkSpots: this.spots.map((i) => { return i.getTime() - started; }),
                },
                (data) => {
                    if (angular.isString(data)) {
                        this.$window.location.assign(data);
                    }
                },
                () => { this.busy = false; }
                );
        };

    }

    angular.module(app.myAppName, [app.utilsNg, 'angular-loading-bar'])
        .controller('SkypeRecorder', SkypeRecorder);

}