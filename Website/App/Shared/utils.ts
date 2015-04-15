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

    // Corresponds to dbo.appTypes ('EX....') and Runnymede.Website.Models.ExerciseType (in ExerciseModels.cs)
    export class ExerciseType {
        static AudioRecording = 'EXAREC';
        static WritingPhoto = 'EXWRPH';

        public static formatLength = (length: number, type: string) => {
            switch (type) {
                case ExerciseType.AudioRecording:
                    return app.formatMsec(length) + ' min:sec';
                    break;
                case ExerciseType.WritingPhoto:
                    return '' + length + ' words';
                    break;
                default:
                    return '' + length;
            };
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

    export function isMobileDevice() {
        var ver = window.navigator.appVersion;
        var ua = window.navigator.userAgent.toLowerCase();
        var mobile = (ver.indexOf("iPad") != -1)
            || (ver.indexOf("iPhone") != -1)
            || (ua.indexOf("android") != -1)
            || (ua.indexOf("ipod") != -1)
            || (ua.indexOf("windows ce") != -1)
            || (ua.indexOf("windows phone") != -1);
        return !!mobile;
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
        arr.some(function (value, index) {
            return test.call(ctx, value, index, arr) ? ((result = value), true) : false;
        });
        return result;
    }

    export function arrRemove<T>(arr: T[], item: T): T {
        var res: T = null;
        var i = arr.indexOf(item); // IE9+
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
    export function arrGroupBy<T>(arr: T[], keySelector: (T) => any) {
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

}