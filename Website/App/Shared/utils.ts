/// <reference path="../../scripts/typings/moment/moment.d.ts" />
module app {

    export var DateTimeFormat = 'DD MMM YYYY HH:mm';
    export var BlobDomainName = 'englmdata.blob.core.windows.net'; // Custom domain mapping does not support HTTPS.
    export var notAuthenticatedMessage = 'Please log in to enable this feature.';

    export function accountUrl(path?: string) { return '/account/' + (path || ''); };
    export function exercisesUrl(path?: string) { return '/exercises/' + path || ''; };
    export function reviewsUrl(path?: string) { return '/reviews/' + (path || ''); };

    export function accountsApiUrl(path?: string) { return '/api/accounts/' + (path || ''); };
    export function converstionsApiUrl(path?: string) { return '/api/converstions/' + (path || ''); };
    export function exercisesApiUrl(path?: string) { return '/api/exercises/' + (path || ''); };
    export function friendsApiUrl(path?: string) { return '/api/friends/' + (path || ''); };
    export function pickapicApiUrl(path?: string) { return '/api/pickapic/' + (path || ''); };
    export function copycatApiUrl(path?: string) { return '/api/copycat/' + (path || ''); };
    export function libraryApiUrl(path?: string) { return '/api/library/' + (path || ''); };
    export function reviewsApiUrl(path?: string) { return '/api/reviews/' + (path || ''); };
    export function sessionsApiUrl(path?: string) { return '/api/sessions/' + (path || ''); };

    // Corresponds to dbo.appTypes ('AR....') and Runnymede.Common.Models.ArtifactType (in ExerciseModels.cs)
    export class ArtifactType {
        static Mp3 = 'ARMP3_';
        static Jpeg = 'ARJPEG';

        public static formatLength = (length: number, artifactType: string) => {
            switch (artifactType) {
                case ArtifactType.Mp3:
                    return app.formatMsec(length) + ' min:sec';
                    break;
                case ArtifactType.Jpeg:
                    return '' + length + ' words';
                    break;
                default:
                    return '' + length;
            };
        };
    }

    // Corresponds to dbo.appTypes ('SV....') and Runnymede.Common.Models.ServiceType (in ExerciseModels.cs)
    export class ServiceType {
        static IeltsWritingTask1 = 'SVRIW1';
        static IeltsWritingTask2 = 'SVRIW2';
        static IeltsSpeaking = 'SVRIS_';
        static IeltsReading = 'SVRIR_';
        static IeltsListening = 'SVRIL_';

        public static getIcon = (serviceType: string) => {
            switch (serviceType) {
                case ServiceType.IeltsWritingTask1:
                    return {
                        name: 'fa-pencil-square-o',
                        title: 'IELTS Writing Task 1'
                    }
                    break;
                case ServiceType.IeltsWritingTask2:
                    return {
                        name: 'fa-pencil',
                        title: 'IELTS Writing Task 2'
                    }
                    break;
                case ServiceType.IeltsSpeaking:
                    return {
                        name: 'fa-comment-o',
                        title: 'IELTS Speaking'
                    }
                    break;
                case ServiceType.IeltsReading:
                    return {
                        name: 'fa-newspaper-o',
                        title: 'IELTS Reading'
                    }
                    break;
                case ServiceType.IeltsListening:
                    return {
                        name: 'fa-volume-up',
                        title: 'IELTS Listening'
                    }
                    break;
                default:
                    return null;
            }
        };
    }

    /** The following properties are passed always: id, displayName, isTeacher. userName is passed only over a secure connection. */
    export function getSelfUser() {
        // Passed by the controller via the page.
        return <app.IUser>app['selfUserParam'];
    };

    export function isAuthenticated() {
        //return Boolean(getSelfUser()); // returns a Boolean object, not a boolean literal;
        return getSelfUser() ? true : false;
    };

    export function getLoginLink() {
        return 'https://' + window.document.location.hostname + '/account/login';
    }

    function isMobile() {
        return navigator.userAgent.toLowerCase().indexOf("mobile") >= 0;
    }

    export function isMobileDevice() {
        var ver = window.navigator.appVersion;
        var ua = window.navigator.userAgent.toLowerCase();
        var mobile =
            (ua.indexOf("android") >= 0)
            || (ver.indexOf("iphone") >= 0)
            || (ver.indexOf("ipad") >= 0)
            || (ua.indexOf("ipod") >= 0)
            //|| (ua.indexOf("windows ce") >= 0)
            || (ua.indexOf("windows phone") >= 0)
            || (ua.indexOf("mobile") >= 0)
            ;
        return !!mobile;
    }

    export function captureSupported(accept: string) {
        // element will be garbage-collected on function return.
        //var element = (<Document>(<any>this.$document[0])).createElement('input'); 
        var element = document.createElement('input');
        element.setAttribute('type', 'file');
        element.setAttribute('accept', accept);
        /* Working Draft +http://www.w3.org/TR/2012/WD-html-media-capture-20120712/ described string values for the capture attribute.
         * Candidate Recommendation +http://www.w3.org/TR/2013/CR-html-media-capture-20130509/ specifies that the capture attribute is of type boolean.
         */
        element.setAttribute('capture', <any>true);
        return element.hasAttribute('accept') && element.hasAttribute('capture');
    }

    function internalFormatDate(val: string, local: boolean): string {
        if (val && val.length > 0) {
            var m = local ? moment.utc(val) : moment(val);
            return m.isValid() ? (local ? m.local() : m).format(DateTimeFormat) : null;
        }
        else {
            return null;
        }
    };

    export function formatDate(val: string): string {
        return internalFormatDate(val, false);
    };

    export function formatDateLocal(valUtc: string): string {
        return internalFormatDate(valUtc, true);
    };

    export function formatDateHuman(val: any) {
        var m = val ? moment(val) : null;
        return moment.isMoment(m) && m.isValid() ? m.calendar() : null;
    };

    export function formatDateAgo(val: any) {
        var m = val ? moment(val) : null;
        return moment.isMoment(m) && m.isValid() ? m.fromNow(true) : null;
    };

    export function formatMsec(valMsec: number): string {
        var min = Math.floor(valMsec / 60000);
        var sec = Math.round((valMsec - min * 60000) / 1000);
        return '' + min + ':' + (sec < 10 ? '0' : '') + sec;
    };

    export function formatMoney(val: any): string {
        var num = numberToMoney(val);
        return num == null ? '' : '' + num;
    };

    export function numberToMoney(val: any): number {
        return isNumber(val) ? Number((Math.round(val * 100) / 100).toFixed(2)) : null;
    };

    /** Fixed length, like '0001' */
    export function formatFixedLength(val: any, length: number) {
        return (new Array(length + 1).join('0') + val).slice(-length);
    }

    // Corresponds to Runnymede.Common.Utils.KeyUtils.IntToKey()
    /** Fixed length 10 digits prepended with zeros if needed, like '0000000001' */
    export function intToKey(val: number) {
        return isNumber(val) ? formatFixedLength(val, 10) : null;
    }

    // angular.isNumber() tests only typeof. It allows for the NaN value which technically is of type Number. So it returns true for +'abc'.
    export function isNumber(val: any) {
        var num = +val; // +null is 0; +undefined is NaN; +'abc' is NaN
        return (val !== null) && (typeof num === 'number') && !isNaN(num);
    }

    export function isValidAmount(val: string): boolean {
        return val && val.length > 0 && /^\d+(?:(?:\.|,)\d{1,2})?$/.test(val);
    };

    export function isUndefinedOrNull(val) {
        return (typeof val === 'undefined') || (val === null);
    };

    export interface ILocalTimeInfo {
        time: string;
        timezoneOffset: number;
    };

    // We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone.
    export function getLocalTimeInfo(): ILocalTimeInfo {
        var d = new Date();
        return {
            time: encodeLocalTime(d),
            timezoneOffset: d.getTimezoneOffset(),
        }
    };

    // LocalTime is a custom format. Its main purpose is to send timezone-neutral local time from client to server in an unambiguous format. 
    // Corresponds to Runnymede.Common.Utils.KeyUtils.EncodeLocalTime()
    export function encodeLocalTime(d: Date) {
        return '' + d.getFullYear() + '/' + (d.getMonth() + 1) + '/' + d.getDate() + '/' + d.getHours() + '/' + d.getMinutes() + '/' + d.getSeconds();
    };

    // Find an array element satisfying the test. Pure JavaScript. IE9+
    export function arrFind<T>(arr: T[], test: (value: T, index: number, array: T[]) => boolean, ctx?: any) {
        var result: T = null;
        (arr || []).some(function (value, index) {
            return test.call(ctx, value, index, arr) ? ((result = value), true) : false;
        });
        return result;
    }

    export function arrRemove<T>(arr: T[], item: T): T {
        var res: T = null;
        var i = (arr || []).indexOf(item); // IE9+
        if (i >= 0) {
            res = arr.splice(i, 1)[0];
        }
        return res;
    }

    // +http://stackoverflow.com/questions/2450954/how-to-randomize-shuffle-a-javascript-array
    export function arrShuffle(arr: any[]) {
        var curr = arr.length, temp, rand;
        while (curr !== 0) {
            rand = Math.floor(Math.random() * curr);
            curr -= 1;
            temp = arr[curr];
            arr[curr] = arr[rand];
            arr[rand] = temp;
        }
        return arr;
    }

    // +http://stackoverflow.com/questions/1890203/unique-for-arrays-in-javascript
    export function arrUniqueScalar(arr: any[]) {
        var hash = {}, res = [];
        for (var i = 0, l = arr.length; i < l; ++i) {
            if (!hash.hasOwnProperty(arr[i])) {
                hash[arr[i]] = true;
                res.push(arr[i]);
            }
        }
        return res;
    }

    // +http://codereview.stackexchange.com/questions/37028/grouping-elements-in-array-by-multiple-properties
    export function arrGroupBy<T>(arr: T[], keySelector: (item: T) => any) {
        var groups = {};
        arr.forEach((i) => {
            var group = JSON.stringify(keySelector(i));
            groups[group] = groups[group] || [];
            groups[group].push(i);
        });
        return Object.keys(groups).map((i) => { return groups[i]; });
    };

    export function logError(data: any, defaultMessage?: any) {
        var m = '' + defaultMessage;
        if (data) {
            // On staus=500 "Internal Server Error" 
            if (data.data) {
                data = data.data;
            }
            var em = data.exceptionMessage ? ('' + data.exceptionMessage) : '';
            // SQL stored procedures return custom formatted error messages with parameter values at the beginning of the message.
            var i = em.indexOf('::');
            if (i > -1) {
                em = em.substring(i + 2);
            }
            m = em ? em : (data.message ? data.message : (data.error_description ? data.error_description : ''));
        }
        toastr.error('Error. ' + m);
    };

    export function isDevHost() {
        return (document.location.hostname.indexOf('dev')) === 0 && (document.location.hostname[4] === '.');
    };

    export function getBlobUrl(containerName: string, blobName: string) {
        if (containerName && blobName) {
            var hostname = document.location.hostname;
            //var protocol = isLocal ? 'http:' : document.location.protocol;
            var protocol = document.location.protocol;
            //var root = isLocal ? '127.0.0.1:10000/devstoreaccount1' : BlobDomainName;
            return app.isDevHost()
                ? protocol + '//' + hostname + '/api/exercises/artifact/' + containerName + '?blobName=' + blobName
                : protocol + '//' + BlobDomainName + '/' + containerName + '/' + blobName;
        }
        else
            return null;
    };

    export function getAvatarSmallUrl(id: number) {
        return getBlobUrl('user-avatars-small', app.intToKey(id));
    }

    export function getAvatarLargeUrl(id: number) {
        return getBlobUrl('user-avatars-large', app.intToKey(id));
    }

    export function soundBeep() {
        var sound = new Audio("data:audio/wav;base64,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qJrx4KOQjahZxWbcZgztj2c49nKmkId44S71j0c8eV9yDK6uPRzx5X18eDvjvQ6yKo9ZSS6l//8elePK/Lf//IInrOF/FvDoADYAGBMGb7FtErm5MXMlmPAJQVgWta7Zx2go+8xJ0UiCb8LHHdftWyLJE0QIAIsI+UbXu67dZMjmgDGCGl1H+vpF4NSDckSIkk7Vd+sxEhBQMRU8j/12UIRhzSaUdQ+rQU5kGeFxm+hb1oh6pWWmv3uvmReDl0UnvtapVaIzo1jZbf/pD6ElLqSX+rUmOQNpJFa/r+sa4e/pBlAABoAAAAA3CUgShLdGIxsY7AUABPRrgCABdDuQ5GC7DqPQCgbbJUAoRSUj+NIEig0YfyWUho1VBBBA//uQZB4ABZx5zfMakeAAAAmwAAAAF5F3P0w9GtAAACfAAAAAwLhMDmAYWMgVEG1U0FIGCBgXBXAtfMH10000EEEEEECUBYln03TTTdNBDZopopYvrTTdNa325mImNg3TTPV9q3pmY0xoO6bv3r00y+IDGid/9aaaZTGMuj9mpu9Mpio1dXrr5HERTZSmqU36A3CumzN/9Robv/Xx4v9ijkSRSNLQhAWumap82WRSBUqXStV/YcS+XVLnSS+WLDroqArFkMEsAS+eWmrUzrO0oEmE40RlMZ5+ODIkAyKAGUwZ3mVKmcamcJnMW26MRPgUw6j+LkhyHGVGYjSUUKNpuJUQoOIAyDvEyG8S5yfK6dhZc0Tx1KI/gviKL6qvvFs1+bWtaz58uUNnryq6kt5RzOCkPWlVqVX2a/EEBUdU1KrXLf40GoiiFXK///qpoiDXrOgqDR38JB0bw7SoL+ZB9o1RCkQjQ2CBYZKd/+VJxZRRZlqSkKiws0WFxUyCwsKiMy7hUVFhIaCrNQsKkTIsLivwKKigsj8XYlwt/WKi2N4d//uQRCSAAjURNIHpMZBGYiaQPSYyAAABLAAAAAAAACWAAAAApUF/Mg+0aohSIRobBAsMlO//Kk4soosy1JSFRYWaLC4qZBYWFRGZdwqKiwkNBVmoWFSJkWFxX4FFRQWR+LsS4W/rFRb/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////VEFHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAU291bmRib3kuZGUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMjAwNGh0dHA6Ly93d3cuc291bmRib3kuZGUAAAAAAAAAACU=");
        sound.play();
    };

    export function soundHit() {
        var a = new Audio("data:audio/wav;base64,UklGRtQAAABXQVZFZm10IBAAAAABAAEARKwAAIhYAQACABAAZGF0YbAAAACLVqdX91Y/VHpQnkzxSDdFeUEjPgg8yztGPXA/N0F0QglE90Z5S/tQKlZcWhpeX2IqZyVrZm0LbuJtDW1ba4hoa2SdXmRXw09MSbREhkGnPs87GjqjOg09UT8FQLc/6z9IQdZC70L1P4k5zjDIJ14fiRbrCxIAz/Uc8KrvH/J58yfxR+sH5HzdU9hr1G/RNc/pzUrNaswbysjGuMR8xhbMA9MT2APa49nS2aXaRttD2g==");
        a.play();
    }

    //export function sound2() {
    //    var track = '';
    //    for (var i = 0; i < 8;) {
    //        i++;
    //        var k, l;
    //        for (k = l = 11025; k--;) {
    //            track += String.fromCharCode(
    //                Math.sin(k / 44100 * 2 * Math.PI * 587.33)
    //                * Math.min((l - k) / 83, k / l)
    //                * (i % 2 && i % 8 - 3 ? 99 : 33) + 128);
    //        }
    //    }
    //    var player = new Audio('data:audio/wav;base64,UklGRkRiBQBXQVZFZm10IBAAAAA\BAAEARKwAAESsAAABAAgAZGF0YSBi' + btoa('\5\0' + track));
    //    player.play();
    //}

}