module app {

    interface IYtPlayerDef {
        width: number;
        height: number;
        idOfElementToReplace: string;
        onReadyCallback: (event: YT.EventArgs) => void;
        onErrorCallback?: (event: YT.EventArgs) => void;
        onStateChangeCallback?: (event: YT.EventArgs) => void;
    }

    var playersToCreate: IYtPlayerDef[] = [];

    export function createYouTubePlayer($window: angular.IWindowService, width: number, height: number, idOfElementToReplace: string,
        onReadyCallback: (event: YT.EventArgs) => void,
        onErrorCallback?: (event: YT.EventArgs) => void,
        onStateChangeCallback?: (event: YT.EventArgs) => void
        ) {

        var def = {
            width: width,
            height: height,
            idOfElementToReplace: idOfElementToReplace,
            onReadyCallback: onReadyCallback,
            onErrorCallback: onErrorCallback,
            onStateChangeCallback: onStateChangeCallback,
        };

        playersToCreate.push(def);

        if (angular.isDefined((<any>$window).YT)) {
            createPlayers();
        }
        else {
            var document = $window.document;
            var tagId = 'myYoutubeIframeApiScript';
            var element = document.getElementById(tagId);
            if (!element) {
                // onYouTubeIframeAPIReady() is called by the YouTube script after the script is loaded.
                (<any>$window).onYouTubeIframeAPIReady = createPlayers;
                // As in the example on +https://developers.google.com/youtube/iframe_api_reference
                var tag = document.createElement('script');
                tag.id = tagId;
                tag.src = 'https://www.youtube.com/iframe_api';
                var firstScriptTag = document.getElementsByTagName('script')[0];
                firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
            }
        };

        function createPlayers() {
            while (playersToCreate.length) {
                var def = playersToCreate.shift();
                createPlayer(def);
            }
        };

        function createPlayer(def: IYtPlayerDef) {
            var origin = $window.location.protocol + '//' + $window.location.hostname;

            var playerVars: YT.PlayerVars = {
                enablejsapi: 1,
                iv_load_policy: 3,
                modestbranding: 1,
                origin: origin,
                rel: 0,
                showinfo: 0,
                theme: 'light'
            };

            var events = <YT.Events>{
                onReady: (event) => {
                    def.onReadyCallback(event);
                },
                onStateChange: (event) => {
                    (def.onStateChangeCallback || angular.noop)(event);
                },
                onError: (event) => {
                    (def.onErrorCallback || angular.noop)(event);
                },
            };

            var playerOptions: YT.PlayerOptions = {
                width: def.width,
                height: def.height, // ratio 4:3 + 35px for the toolbar
                videoId: null,
                playerVars: playerVars,
                events: events
            };

            new YT.Player(def.idOfElementToReplace, playerOptions);
        };
    };

    export function getYtVideoId(url: string) {
        return url
            && (url.length === 43)
            && (url.substr(0, 32) === 'https://www.youtube.com/watch?v=') // The canonical format
            && url.substr(32, 11);
    };

} // end of module app