var App;
(function (App) {
    (function (Utils) {
        function exercisesUrl(path) {
            return '/exercises/' + path || '';
        }
        Utils.exercisesUrl = exercisesUrl;
        ;
        function reviewsUrl(path) {
            return '/reviews/' + (path || '');
        }
        Utils.reviewsUrl = reviewsUrl;
        ;
        function accountUrl(path) {
            return '/account/' + (path || '');
        }
        Utils.accountUrl = accountUrl;
        ;
        function loginUrl() {
            return '/Token';
        }
        Utils.loginUrl = loginUrl;
        ;
        function relationshipsUrl(path) {
            return '/relationships/' + (path || '');
        }
        Utils.relationshipsUrl = relationshipsUrl;
        ;

        function statisticsApiUrl(path) {
            return '/api/StatisticsApi/' + (path || '');
        }
        Utils.statisticsApiUrl = statisticsApiUrl;
        ;
        function exercisesApiUrl(path) {
            return '/api/ExercisesApi/' + (path || '');
        }
        Utils.exercisesApiUrl = exercisesApiUrl;
        ;
        function remarksApiUrl(path) {
            return '/api/RemarksApi/' + (path || '');
        }
        Utils.remarksApiUrl = remarksApiUrl;
        ;
        function reviewsApiUrl(path) {
            return '/api/ReviewsApi/' + (path || '');
        }
        Utils.reviewsApiUrl = reviewsApiUrl;
        ;
        function balanceApiUrl(path) {
            return '/api/balanceApi/' + (path || '');
        }
        Utils.balanceApiUrl = balanceApiUrl;
        ;
        function accountApiUrl(path) {
            return '/api/AccountApi/' + (path || '');
        }
        Utils.accountApiUrl = accountApiUrl;
        ;
        function relationshipsApiUrl(path) {
            return '/api/RelationshipsApi/' + (path || '');
        }
        Utils.relationshipsApiUrl = relationshipsApiUrl;
        ;

        // Header for authentication with WebAPI
        var accessTokenKey = 'accessToken';
        var timezoneOffsetMinKey = 'timezoneOffsetMin';

        function setAccessToken(token, persistent) {
            // sessionStorage and session cookie work differently across tabs. // See +http://dev.w3.org/html5/webstorage/#introduction
            // If the user opens a new tab, she is still authorized to view the page, but AJAX requests from that page will fail.
            // We do not use sessionStorage to compliment the cookie behavior.
            //if (persistent) {
            localStorage[accessTokenKey] = token;
            //} else {
            //    sessionStorage[accessTokenKey] = token;
            //}
        }
        Utils.setAccessToken = setAccessToken;
        ;

        function getSecurityHeader() {
            var token = sessionStorage[accessTokenKey] || localStorage[accessTokenKey];
            if (token) {
                return { "Authorization": "Bearer " + token };
            }
            return {};
        }
        Utils.getSecurityHeader = getSecurityHeader;

        //export function setTimezoneOffsetMin(value) {
        //    timezoneOffsetMin = value;
        //    localStorage[timezoneOffsetMinKey] = value;
        //};
        //export function getTimezoneOffsetMin() {
        //    return timezoneOffsetMin;
        //}
        Utils.TimezoneOffsetMin;
        var timezoneOffsetMin = localStorage[timezoneOffsetMinKey];
        Object.defineProperty(App.Utils, "TimezoneOffsetMin", {
            get: function () {
                return timezoneOffsetMin;
            },
            set: function (val) {
                timezoneOffsetMin = val;
                localStorage[timezoneOffsetMinKey] = val;
            },
            enumerable: true,
            configurable: true
        });

        // Returns a Deffered object
        function ajaxRequest(type, url, data) {
            return $.ajax({
                type: type,
                url: url,
                data: ko.toJSON(data),
                dataType: 'json',
                contentType: 'application/json',
                cache: false,
                headers: getSecurityHeader()
            });
        }
        Utils.ajaxRequest = ajaxRequest;

        // Returns a Deffered object
        function ajaxGet(url) {
            return ajaxRequest('GET', url);
        }
        Utils.ajaxGet = ajaxGet;

        function ajaxPostAsForm(url, data) {
            return $.ajax({
                type: 'POST',
                url: url,
                data: data,
                cache: false
            });
        }
        Utils.ajaxPostAsForm = ajaxPostAsForm;

        // ko.datasource
        function dataSource(url, ctor) {
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
                    headers: getSecurityHeader()
                }).done(function (dto) {
                    if (dto.items) {
                        this($.map(dto.items, function (i) {
                            return ctor(i);
                        }));
                    }
                    this.pager.totalCount(dto.totalCount);
                }).always(function () {
                    App.Utils.activityIndicator(false);
                });
            };
        }
        Utils.dataSource = dataSource;

        function parseDate(val) {
            var num = Date.parse(val);
            if (!isNaN(num)) {
                return new Date(num);
            } else {
                if (val) {
                    var m = moment(val);
                    return m.isValid() ? m.toDate() : null;
                    //var d = m.toDate();
                    //return (d.toString() === 'Invalid Date') ? null : d;
                } else {
                    return null;
                }
            }
        }
        Utils.parseDate = parseDate;

        Utils.dateTimeFormat = 'D MMM YYYY HH:mm';

        function internalFormatDate(val, local) {
            if (val && val.length > 0) {
                var m = local ? moment.utc(val) : moment(val);
                return m.isValid() ? (local ? m.local() : m).format(Utils.dateTimeFormat) : null;
            } else {
                return null;
            }
        }

        function formatDate(val) {
            return internalFormatDate(val, false);
        }
        Utils.formatDate = formatDate;

        function formatDateLocal(valUtc) {
            return internalFormatDate(valUtc, true);
        }
        Utils.formatDateLocal = formatDateLocal;

        function formatMoney(val) {
            return numberToMoney(Number(val));
        }
        Utils.formatMoney = formatMoney;

        function numberToMoney(val) {
            return isNaN(val) ? '' : (Math.round(val * 100) / 100).toFixed(2);
        }
        Utils.numberToMoney = numberToMoney;

        function formatMsec(valMsec) {
            var min = Math.floor(valMsec / 60000);
            var sec = Math.round((valMsec - min * 60000) / 1000);
            return '' + min + ':' + (sec < 10 ? '0' : '') + sec;
        }
        Utils.formatMsec = formatMsec;
        ;

        function isValidAmount(val) {
            return val && val.length > 0 && /^\d+(?:(?:\.|,)\d{1,2})?$/.test(val);
        }
        Utils.isValidAmount = isValidAmount;
        ;

        function getNoCacheUrl() {
            return '?_=' + safeDateNow();
        }
        Utils.getNoCacheUrl = getNoCacheUrl;

        function safeDateNow() {
            // IE8 does not support Date.now() +http://afuchs.tumblr.com/post/23550124774/date-now-in-ie8
            return Date.now ? Date.now() : (new Date()).valueOf();
        }
        Utils.safeDateNow = safeDateNow;

        // We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone.
        function getLocalTimeInfo() {
            var d = new Date();
            return {
                time: '' + d.getFullYear() + '/' + (d.getMonth() + 1) + '/' + d.getDate() + '/' + d.getHours() + '/' + d.getMinutes() + '/' + d.getSeconds(),
                timeZoneOffset: d.getTimezoneOffset()
            };
        }
        Utils.getLocalTimeInfo = getLocalTimeInfo;

        // Find an array element satisfying the test. Pure JavaScript.
        function find(arr, test, ctx) {
            var result = null;
            arr.some(function (el, i) {
                return test.call(ctx, el, i, arr) ? ((result = el), true) : false;
            });
            return result;
        }
        Utils.find = find;

        function activityIndicator(show) {
            // NETEYE Activity Indicator in knockout.activity.js
            ($(document.body)).activity(show);
        }
        Utils.activityIndicator = activityIndicator;

        function logAjaxError(jqXHR, defaultMessage) {
            var r = jqXHR.responseJSON;
            var m;
            if (r) {
                var em = r.exceptionMessage ? ('' + r.exceptionMessage) : '';

                // SQL stored procedures return custom formatted error messages with values at the beginning of the message.
                var i = em.indexOf('::');
                if (i > -1) {
                    em = em.substring(i + 2);
                }
                m = em ? em : (r.message ? r.message : (r.error_description ? r.error_description : ''));
            }

            toastr.error('Error. ' + r ? m : defaultMessage);
        }
        Utils.logAjaxError = logAjaxError;
    })(App.Utils || (App.Utils = {}));
    var Utils = App.Utils;
})(App || (App = {}));
//# sourceMappingURL=utils.js.map
