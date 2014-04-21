module App.Utils {

    export function exercisesUrl(path?: string) { return '/exercises/' + path || ''; };
    export function reviewsUrl(path?: string) { return '/reviews/' + (path || ''); };
    export function accountUrl(path?: string) { return '/account/' + (path || ''); };
    export function loginUrl() { return '/Token'; };
    export function relationshipsUrl(path?: string) { return '/relationships/' + (path || ''); };

    export function statisticsApiUrl(path?: string) { return '/api/StatisticsApi/' + (path || ''); };
    export function exercisesApiUrl(path?: string) { return '/api/ExercisesApi/' + (path || ''); };
    export function remarksApiUrl(path?: string) { return '/api/RemarksApi/' + (path || ''); };
    export function reviewsApiUrl(path?: string) { return '/api/ReviewsApi/' + (path || ''); };
    export function balanceApiUrl(path?: string) { return '/api/balanceApi/' + (path || ''); };
    export function accountApiUrl(path?: string) { return '/api/AccountApi/' + (path || ''); };
    export function relationshipsApiUrl(path?: string) { return '/api/RelationshipsApi/' + (path || ''); };
    export function teachersApiUrl(path?: string) { return '/api/TeachersApi/' + (path || ''); };
    export function sessionsApiUrl(path?: string) { return '/api/SessionsApi/' + (path || ''); };


    // Returns a Deffered object
    export function ajaxRequest(type: string, url: string, data?: any) {
        var settings: JQueryAjaxSettings = {
            type: type,
            url: url,
            data: ko.toJSON(data),
            dataType: 'json',
            contentType: 'application/json',
            cache: false,
        };
        return $.ajax(settings);
    }

    // Returns a Deffered object
    export function ajaxGet(url: string) {
        return ajaxRequest('GET', url);
    }

    export function ajaxPostAsForm(url: string, data?: any) {
        return $.ajax({
            type: 'POST',
            url: url,
            data: data,
            cache: false
        });
    }

    // ko.datasource
    export function dataSource(url, ctor) {
        return function () {
            App.Utils.activityIndicator(true);

            $.ajax({
                type: 'GET',
                url: url,
                data: {
                    offset: this.pager.limit() * (this.pager.page() - 1),
                    limit: this.pager.limit()
                },
                context: this,
                dataType: 'json',
            })
                .done(function (dto) {
                    if (dto.items) {
                        this($.map(dto.items, function (i) { return ctor(i); }));
                    }
                    this.pager.totalCount(dto.totalCount);
                })
                .always(function () {
                    App.Utils.activityIndicator(false);
                });
        };
    }

    export function parseDate(val: string): Date {
        var num = Date.parse(val);
        if (!isNaN(num)) {
            return new Date(num);
        }
        else {
            if (val) {
                var m = moment(val);
                return m.isValid() ? m.toDate() : null;
                //var d = m.toDate();
                //return (d.toString() === 'Invalid Date') ? null : d;
            }
            else {
                return null;
            }
        }
    }

    export var dateTimeFormat = 'D MMM YYYY HH:mm';

    function internalFormatDate(val: string, local: boolean): string {
        if (val && val.length > 0) {
            var m = local ? moment.utc(val) : moment(val);
            return m.isValid() ? (local ? m.local() : m).format(dateTimeFormat) : null;
        }
        else {
            return null;
        }
    }

    export function formatDate(val: string): string {
        return internalFormatDate(val, false);
    }

    export function formatDateLocal(valUtc: string): string {
        return internalFormatDate(valUtc, true);
    }

    export function formatMoney(val: string): string {
        return numberToMoney(Number(val));
    }

    export function numberToMoney(val: number): string {
        return isNaN(val) ? '' : (Math.round(val * 100) / 100).toFixed(2);
    }

    export function formatMsec(valMsec: number): string {
        var min = Math.floor(valMsec / 60000);
        var sec = Math.round((valMsec - min * 60000) / 1000);
        return '' + min + ':' + (sec < 10 ? '0' : '') + sec;
    };

    export function isValidAmount(val: string): boolean {
        return val && val.length > 0 && /^\d+(?:(?:\.|,)\d{1,2})?$/.test(val);
    };

    export function getNoCacheUrl() {
        return '_=' + safeDateNow();
    }

    export function safeDateNow() {
        // IE8 does not support Date.now() +http://afuchs.tumblr.com/post/23550124774/date-now-in-ie8
        return Date.now ? Date.now() : (new Date).valueOf();
    }

    export interface ILocalTimeInfo {
        time: string;
        timezoneOffset: number;
    }

    // We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone.
    export function getLocalTimeInfo(): ILocalTimeInfo {
        var d = new Date();
        return {
            time: '' + d.getFullYear() + '/' + (d.getMonth() + 1) + '/' + d.getDate() + '/' + d.getHours() + '/' + d.getMinutes() + '/' + d.getSeconds(),
            timezoneOffset: d.getTimezoneOffset()
        }
    }

    // Find an array element satisfying the test. Pure JavaScript.
    export function find(arr, test, ctx?) {
        var result = null;
        arr.some(function (el, i) {
            return test.call(ctx, el, i, arr) ? ((result = el), true) : false;
        });
        return result;
    }

    export function activityIndicator(show: boolean) {
        // NETEYE Activity Indicator in knockout.activity.js
        (<any>$(document.body)).activity(show);
    }

    export function logAjaxError(jqXHR: any, defaultMessage: string) {
        logError(jqXHR.responseJSON, defaultMessage);
    }

    export function logError(data: any, defaultMessage: any) {
        var m = defaultMessage;
        if (data) {
            var em = data.exceptionMessage ? ('' + data.exceptionMessage) : '';
            // SQL stored procedures return custom formatted error messages with values at the beginning of the message.
            var i = em.indexOf('::');
            if (i > -1) {
                em = em.substring(i + 2);
            }
            m = em ? em : (data.message ? data.message : (data.error_description ? data.error_description : ''));
        }
        toastr.error('Error. ' + m);
    }


}