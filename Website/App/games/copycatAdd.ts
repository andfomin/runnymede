module app.games {

    export class CopycatAdd extends app.CtrlBase {

        url: string;
        start: Date; // timepicker works with hour:min. We need min:sec.
        finish: Date;
        transcript: string;
        player: YT.Player = null;
        scene: boolean = true;
        resource: IResource;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$window];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService,
            private $window: ng.IWindowService
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            if (this.authenticated) {
                app.library.createYouTubePlayer($window, 240, 240, 'toBeReplacedByYoutubeIframe',
                    (event: YT.EventArgs) => { this.player = event.target; },
                    (event: YT.EventArgs) => { toastr.error('Player error ' + event.data); }
                    );
            }

            this.start = new Date(0, 0, 1, 0, 0, 0, 0); // Month in JS is zero-based, day is one-based.
            this.finish = this.start;
            /* ----- End of constructor  ----- */
        }

        private timeToNumber(value: Date) {
            // timepicker works with hour:min. We mean min:sec. We store and the player API accepts total seconds as a number.
            return 60 * value.getHours() + value.getMinutes();
        }

        private getStart = () => {
          return this.timeToNumber(this.start)
       };

        private getFinish = () => {
          return this.timeToNumber(this.finish)
        };

        validate = () => {
            app.ngHttpPost(this.$http,
                app.copycatApiUrl('validate'),
                {
                    url: this.url,
                },
                (data) => {
                    if (app.library.isYoutube(data)) {
                        this.resource = data;
                        if (this.player && this.resource) {
                            this.player.cueVideoById(this.resource.naturalKey);
                        }
                    }
                    else {
                        this.resource = null;
                        toastr.error('Video not found');
                    }

                }
                );
        };

        changed = () => {
            if (this.player) {
                this.player.pauseVideo();
            }
        };

        isFragmentValid = () => {
            var start = this.getStart();
            var finish = this.getFinish();
            return (start >= 0) && (start < finish);
        };

        play = () => {
            if (this.player && app.library.isYoutube(this.resource) && this.isFragmentValid()) {
                this.player.loadVideoById({
                    videoId: this.resource.naturalKey,
                    startSeconds: this.getStart(),
                    endSeconds: this.getFinish(),
                });
            }
        };

        canSave = () => {
            return this.url && (!this.scene || this.isFragmentValid());
        }

        save = () => {
            return app.ngHttpPost(this.$http,
                app.copycatApiUrl('resource'),
                {
                    url: this.url,
                    start: this.scene ? this.timeToNumber(this.start) : null,
                    finish: this.scene ? this.timeToNumber(this.finish) : null,
                    transcript: this.transcript,
                },
                () => { 'The piece is saved. Thank you!' }
                );
        };

    }; // end of class Ctrl

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('CopycatAdd', app.games.CopycatAdd);

} // end of module app.games
