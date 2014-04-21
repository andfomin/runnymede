var App;
(function (App) {
    (function (Utils) {
        function signIn($http, userName, password, persistent, finallyCallback, redirectTo) {
            $http({
                method: 'POST',
                url: Utils.loginUrl(),
                data: {
                    grant_type: 'password',
                    userName: userName,
                    password: password,
                    scope: persistent ? 'persistent_cookie' : 'session_cookie'
                },
                /* The OWIN authintication middleware does not accept JSON. It wants a form. */
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                transformRequest: function (obj) {
                    var str = [];
                    for (var p in obj)
                        str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                    return str.join("&");
                }
            }).success(function (data) {
                // Store the access token for using with WebAPI.
                App.Utils.setAccessToken(data.access_token);

                /* We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                var timeInfo = App.Utils.getLocalTimeInfo();

                App.Utils.ngHttpPost($http, Utils.accountApiUrl('SignedIn'), {
                    persistent: persistent,
                    localTime: timeInfo.time,
                    localTimezoneOffset: timeInfo.timeZoneOffset
                }, function (data) {
                    //App.Utils.TimezoneOffsetMin = data.timezoneOffsetMin;
                }, function () {
                    if (finallyCallback) {
                        finallyCallback();
                    }
                    if (redirectTo) {
                        window.location.replace(redirectTo);
                    }
                });
            }).error(function (data, status) {
                if (finallyCallback) {
                    finallyCallback();
                }
                App.Utils.logNgHttpError(data, status);
            });
        }
        Utils.signIn = signIn;
    })(App.Utils || (App.Utils = {}));
    var Utils = App.Utils;
})(App || (App = {}));
//# sourceMappingURL=utils-account.js.map
