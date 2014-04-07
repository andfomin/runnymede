module App.Utils {

    export function signIn($http: ng.IHttpService, userName: string, password: string, persistent: boolean, finallyCallback?: () => any, redirectTo?: string) {
        $http({
            method: 'POST',
            url: Utils.loginUrl(),
            data: {
                grant_type: 'password',
                userName: userName,
                password: password,
                scope: persistent ? 'persistent_cookie' : 'session_cookie', // Pass our custom value. Scope may be a list of values separated by spaces.
            },
            /* The OWIN authintication middleware does not accept JSON. It wants a form. */
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            transformRequest: function (obj) {
                var str = [];
                for (var p in obj)
                    str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                return str.join("&");
            }
        })
            .success((data) => {
                // Store the access token for using with WebAPI.
                App.Utils.setAccessToken(data.access_token, persistent);
                /* We send the client-side time and the local TimezoneOffset with the form to infer the client's actual time zone. */
                var timeInfo = App.Utils.getLocalTimeInfo();

                App.Utils.ngHttpPost($http,
                    accountApiUrl('SignedIn'),
                    {
                        persistent: persistent,
                        localTime: timeInfo.time,
                        localTimezoneOffset: timeInfo.timeZoneOffset
                    },
                    (data) => {
                        App.Utils.TimezoneOffsetMin = data.timezoneOffsetMin;
                    },
                    () => {
                        if (finallyCallback) {
                            finallyCallback();
                        }
                        if (redirectTo) {
                            window.location.replace(redirectTo);
                        }
                    }
                    );
            })
            .error((data, status) => {
                if (finallyCallback) {
                    finallyCallback();
                }
                App.Utils.logNgHttpError(data, status);
            });
    }

} // end of module
