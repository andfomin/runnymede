module app.games {

    export class Copycat extends app.CtrlBase {

        // these constants are part of the route.
        KindNotViewed = 'NotViewed';
        KindViewed = 'Viewed';

        pagingSession: number;
        player: YT.Player = null;
        kind: string;
        resources: IResource[];
        resource: IResource = null;
        transcript: string;
        publicTranscript: string;
        loop: boolean = false;
        changeVolume: boolean = false;
        isQuiet: boolean = false;
        playerState: number;

        priorities = [
            // null means not veiewed.
            // 0 means viewed but not rated
            { value: 1, icon: 'fa-angle-double-down' },
            { value: 2, icon: 'fa-angle-down' },
            { value: 3, icon: 'fa-angle-up' },
            { value: 4, icon: 'fa-angle-double-up' },
            // 5 means the resource was added by this user
            { value: 5, icon: 'fa-plus' }
        ];

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$window, app.ngNames.$interval, app.ngNames.$modal];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $window: angular.IWindowService,
            private $interval: angular.IIntervalService,
            private $modal: angular.ui.bootstrap.IModalService
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            app.library.createYouTubePlayer($window, 240, 240, 'toBeReplacedByYoutubeIframe',
                (event: YT.EventArgs) => { this.player = event.target; },
                (event: YT.EventArgs) => { toastr.error('Player error ' + event.data); },
                (event: YT.EventArgs) => { this.onPlayerStateChange(event.data); }
                );

            // pagingSession is used for maintaining a repeatable random order.
            this.pagingSession = new Date().getTime() % 1000;

            this.selectKind(this.authenticated ? this.KindViewed : this.KindNotViewed);
            /* ----- End of constructor  ----- */
        }

        private clear = () => {
            this.kind = null;
            this.selectResource(null);
            this.resources = null;
        };

        selectKind = (kind) => {
            this.clear();
            this.kind = kind;
            if (this.isKindNotViewed() || this.isKindViewed()) {
                this.pgLoad();
            }
        };

        selectResource = (resource: IResource) => {
            this.resource = resource;
            this.loop = false;
            this.changeVolume = false;
            this.isQuiet = false;
            this.transcript = null;
            this.publicTranscript = null;
            this.cueVideo(resource);
        };

        pgLoad = () => {
            this.selectResource(null);
            this.resources = null;
            this.pgTotal = 0;

            var getViewedParam = () => {
                switch (this.kind) {
                    case this.KindNotViewed:
                        return false;
                    case this.KindViewed:
                        return true;
                    default:
                        return null;
                }
            };

            if (this.authenticated || this.isKindNotViewed()) {
                app.ngHttpGet(this.$http,
                    app.copycatApiUrl('resources'),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit,
                        viewed: getViewedParam(),
                        session: this.pagingSession,
                    },
                    (data) => {
                        this.resources = data.items;
                        this.pgTotal = data.totalCount;
                    }
                    );
            }
        };

        onPlayerStateChange = (state: number) => {
            /* -1 - unstarted, 0 - ended, 1 - playing, 2 - paused, 3 - buffering, 5 - video cued */
            console.log('state: ' + state);
            var oldState = this.playerState;
            this.playerState = state;

            switch (state) {
                case YT.PlayerState.ENDED:
                    if (this.loop && oldState === YT.PlayerState.PLAYING) {
                        this.replay();
                    }
                    break;
                case YT.PlayerState.PLAYING:
                    if (this.resource.priority === null) {
                        this.resource.priority = 0;
                        app.library.logResourceView(this.$http, this.resource);
                    }
                    break;
                case YT.PlayerState.CUED:
                    // Mobile browsers may block play initiated by a script. The user may need to press the play button manually.
                    this.player.playVideo();
                    break;
                default:
                    ;
            }
        };

        private replay = () => {
            if (this.changeVolume) {
                this.player.setVolume(this.isQuiet ? 25 : 75);
                this.isQuiet = !this.isQuiet;
            }
            // cueVideo()/playVideo() reloads the video metadata and sends analytics back.
            // seekTo() sends analytics back on every replay event anyway but does not reload the video metadata.
            // If the player is paused when the function is called, it will remain paused. If the function is called from another state (playing, video cued, etc.), the player will play the video.
            this.player.seekTo(this.getStart(this.resource), true);
        };

        private cueVideo = (resource: IResource) => {
            if (this.player && resource) {
                var valid = this.isValidResource(resource);
                this.player.cueVideoById({
                    videoId: resource.naturalKey,
                    startSeconds: valid ? this.getStart(resource) : null,
                    endSeconds: valid ? this.getFinish(resource) : null,
                });
            }
        };

        getStart = (resource: IResource) => {
            var s = resource && resource.segment;
            return s ? +s.substr(0, 5) : 0;
        };

        getFinish = (resource: IResource) => {
            var s = resource && resource.segment;
            return s ? +s.substr(5, 5) : 0;
        };

        formatTime = (val: number) => {
            var min = Math.floor(val / 60);
            var sec = val - min * 60;
            return '' + min + ':' + app.formatFixedLength(sec, 2);
        };

        formatSegment = (resource: IResource) => {
            return this.formatTime(this.getStart(resource)) + ' - ' + this.formatTime(this.getFinish(resource));
        };

        isKindNotViewed = () => {
            return this.kind === this.KindNotViewed;
        };

        isKindViewed = () => {
            return this.kind === this.KindViewed;
        };

        isValidResource = (resource: IResource) => {
            var start = this.getStart(resource);
            var finish = this.getFinish(resource);
            return app.library.isYoutube(resource) && (start >= 0) && (start < finish);
        };

        getGroups = () => {
            return (this.resources || []).filter((i) => { return !!i.title; });
        };

        getResources = (resource: IResource) => {
            return this.resources
                .filter((i) => { return (i.naturalKey === resource.naturalKey) && app.library.isYoutube(i); })
                .sort((a, b) => {
                    var sa = this.getStart(a);
                    var sb = this.getStart(b);
                    return (sa > sb) ? 1 : ((sa < sb) ? -1 : 0);
                });
        };

        getGroupIcon = (group: IResource) => {
            var priorities = this.resources
                .filter((i) => { return (i.naturalKey === group.naturalKey) && angular.isNumber(i.priority); })
                .map((i) => { return i.priority; })
            ;
            var max = priorities.length
                ? priorities.reduce((previous, current) => { return (current > previous) ? current : previous; }, 0)
                : null;
            var p = app.arrFind(this.priorities, (i) => { return i.value === max; });
            return p && p.icon;
        };

        setPriority = (resource: IResource, priority: number) => {
            resource.priority = priority;
            app.ngHttpPut(this.$http,
                app.copycatApiUrl(resource.id + '/priority'),
                {
                    priority: priority,
                }
                );
        };

        showPublicTranscript = () => {
            if (this.resource) {
                app.ngHttpGet(this.$http,
                    app.copycatApiUrl('transcript/' + this.resource.id),
                    null,
                    (data) => {
                        this.publicTranscript = (data && data.transcript) || ' ';
                    }
                    );
            }
        };

        canSaveTranscript = () => {
            return this.resource && this.transcript && this.publicTranscript;
        };

        showSaveTranscriptModal = () => {
            app.Modal.openModal(this.$modal,
                'saveTranscriptModal',
                SaveTranscriptModal,
                {
                    resourceId: this.resource && this.resource.id,
                    transcript: this.transcript,
                },
                () => { this.publicTranscript = this.transcript; }
                );
        };

    } // end of class Ctrl

    export class SaveTranscriptModal extends app.Modal {

        internalOk = () => {
            return app.ngHttpPost(this.$http,
                app.copycatApiUrl('transcript/' + this.modalParams.resourceId),
                {
                    transcript: this.modalParams.transcript
                },
                () => { toastr.success('Transcript saved.'); }
                );
        };
    }; // end of class SaveTranscriptModal

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Copycat', Copycat);

} // end of module app.games_copycat
