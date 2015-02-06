module app {

    export interface IScopeWithViewModel extends ng.IScope {
        vm: any;
    };

    export interface IAppRootScopeService extends ng.IRootScopeService {
        // Dynamically change the page title. ?Refactor as a service? See +http://stackoverflow.com/a/17751833/2808337
        pageTitle: string;
        secondaryTitle: string;
        isClockWrong: boolean;
    };

    export class ngNames {
        // from module ng
        public static $compileProvider = '$compileProvider';
        public static $document = '$document';
        public static $filter = '$filter';
        public static $http = '$http';
        public static $interval = '$interval'; // Intervals created by this service must be explicitly destroyed. See the example in the docs.
        public static $location = '$location';
        public static $locationProvider = '$locationProvider';
        public static $log = '$log';
        public static $q = '$q';
        public static $rootScope = '$rootScope';
        public static $scope = '$scope';
        public static $sceDelegateProvider = '$sceDelegateProvider';
        public static $timeout = '$timeout';
        public static $window = '$window';
        // ui-bootstrap
        public static datepickerConfig = 'datepickerConfig';
        public static datepickerPopupConfig = 'datepickerPopupConfig';
        public static $modal = '$modal';
        public static $modalInstance = '$modalInstance';
        // ui-router
        public static $state = '$state';
        public static $stateParams = '$stateParams';
        public static $stateProvider = '$stateProvider';
        public static $urlRouterProvider = '$urlRouterProvider';
        // third-party
        public static jQuery = 'jQuery';
        // Custom
        public static $appRemarks = '$appRemarks';
        public static $appRemarksComparer = '$appRemarksComparer';
        public static $signalRService = '$signalRService';
    };

    export var myAppName = 'myApp';
    export var utilsNg = 'app.utilsNg';

    export var pgLimit: number = 10; // Items per page
    export var notAuthenticatedMessage = 'Please log in to enable this feature.';

    export class CtrlBase {
        busy: boolean = false;
        // Although we have the first level of defence on the client, the server will often accept unauthenticated requests and may return an error message for unavailable features.
        authenticated: boolean;
        loginLink: string;
        pgLimit: number = app.pgLimit; // Items per page
        pgTotal: number = null;
        pgCurrent: number = 1;

        //static $inject = [app.ngNames.$scope];

        constructor(
            $scope: app.IScopeWithViewModel
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;
            this.authenticated = app.isAuthenticated();
            this.loginLink = app.getLoginLink();
            /* ----- End of constructor  ----- */
        }

        pgOffset = (pgCurrent?: number, pgLimit?: number) => {
            return ((pgCurrent || this.pgCurrent || 1) - 1) * (pgLimit || this.pgLimit || 0);
        };

        pgReset = () => {
            this.pgCurrent = 1;
            this.pgTotal = null;
        }

    } // end of class CtrlBase

    export interface IModal {
        canOk: () => any;
        internalOk: () => ng.IPromise<any>;
    }

    export class Modal {

        busy: boolean = false;
        authenticated: boolean;// The server may accept unauthenticated requests for some features but returns a custom error message for the disabled features.
        loginLink: string;

        canOk: () => any = () => { return true; }; // Replaced in descendand classes.
        internalOk: () => ng.IPromise<any>; // Replaced in descendand classes.
        dismissOnError: boolean = false;

        /* +http://www.typescriptlang.org/Content/TypeScript%20Language%20Specification.pdf  Section 8.2.3
        *  Base class static property members can be overridden by derived class static property members of any kind as long as the types are compatible, as described above. */
        static $inject = [app.ngNames.$http, app.ngNames.$modalInstance, app.ngNames.$scope, 'modalParams'];

        constructor(
            public $http: ng.IHttpService,
            public $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
            public $scope: app.IScopeWithViewModel,
            public modalParams: any
            ) {
            $scope.vm = this;
            this.authenticated = app.isAuthenticated();
            this.loginLink = app.getLoginLink(); // 'https://' + $window.document.location.hostname + '/account/login';
        } // ctor

        ok = () => {
            if (this.internalOk
                && !this.busy
                && this.canOk()
                ) {
                this.busy = true;
                this.internalOk()
                    .then<void>(
                    (data) => {
                        //$http.post().success() returns ng.IHttpPromiseCallbackArg<any>
                        var httpData = data && data.data;
                        this.$modalInstance.close(httpData || data);
                    },
                    (reason) => {
                        if (this.dismissOnError) {
                            this.$modalInstance.dismiss(reason);
                        }
                        // app.logError. We usually use app.ngHttpPost in internalOk(), which has its own errorCallback. Avoid duplicate error processing and duplicate toastr baloons.
                    }
                    )
                    .finally(() => { this.busy = false; });
            }
            else {
                this.$modalInstance.close();
            }
        }

        cancel = () => {
            this.$modalInstance.dismiss('cancel');
        };

        /**
         * The successCallback parameter should be passed using the lambda syntax, otherwise the _this will be lost.
         */
        public static openModal(
            $modal: ng.ui.bootstrap.IModalService,
            templateUrl: string,
            controller: new (...args: any[]) => IModal,
            modalParams?: any,
            successCallback?: (data: any) => void,
            backdrop?: any, // true (default???), false (no backdrop), 'static'
            size?: string // 'sm', 'lg'
            ) {
            var options: ng.ui.bootstrap.IModalSettings = {
                templateUrl: templateUrl,
                controller: controller,
                resolve: {
                    modalParams: () => {
                        return modalParams || null;
                    }
                },
                backdrop: angular.isDefined(backdrop) ? backdrop : true,
                size: size,
            };
            var modalInstance = $modal.open(options);
            modalInstance.result.then(
                (data) => { (successCallback || angular.noop)(data); },
                null
                );
            return modalInstance;
        }
    }; // end of class Modal

    export class Help {
        isCollapsed: boolean;
        include: string;

        static $inject = [app.ngNames.$scope];

        constructor(
            $scope: app.IScopeWithViewModel
            )
        /* ----- Constructor  ----- */ {
            $scope.vm = this;
            this.isCollapsed = app.isAuthenticated();
            this.setUrl();
            /* ----- End of constructor  ----- */
        }

        onClick = () => {
            this.isCollapsed = !this.isCollapsed;
            this.setUrl();
        };

        setUrl = () => {
            if (!this.isCollapsed) {
                var host = app.isDevHost() ? '' : ('//' + app.BlobDomainName);
                var file = app['helpFileParam'];
                this.include = host + '/content/help/' + file;
            }
        };

    } // end of class Help

    export interface ISignalREventHandler {
        eventName: string;
        handler: (args: any[]) => void;
    }

    export interface ISignalRService {
        setEventHandlers: (hubName: string, eventHandlers: ISignalREventHandler[]) => void;
        start: () => JQueryPromise<any>;
        stop: () => void;
        invoke: (hubName: string, methodName: string, ...args: any[]) => JQueryDeferred<any>;
        isStarted: () => boolean;
    }

    // The main benefit of wrapping SignalR in this service is to have a singleton connection during the life cicle of the app.
    export class SignalRService implements ISignalRService {

        conn: HubConnection;
        started: boolean = false;

        static $inject = [app.ngNames.jQuery, app.ngNames.$rootScope];

        constructor(
            private $: JQueryStatic,
            private $rootScope: ng.IRootScopeService
            )
        /* ----- Constructor  ----- */ {
            // ConnectionId is keept the same on reconnects.
            this.conn = this.$.hubConnection();
            this.conn.logging = true;
            this.conn.starting(() => { this.started = true; });
            this.conn.disconnected(() => { this.started = false; });
        }

        isStarted = () => {
            return this.started;
        };

        start = () => {
            if (!this.started) {
                // At least one event handler must be added before start() is called. Otherwise events assigned later will not fire;
                return this.conn.start();
            }
            // else return undefined;
        };

        stop = () => {
            this.conn.stop();
        };

        setEventHandlers = (hubName: string, eventHandlers: ISignalREventHandler[]) => {
            /* The hubName is a camel-cased version of the Hub class name on the server (may be overriden by an HubName attribute).
               We call an arbitrary method on the server and the method call is marshalled to the client where it is dispatched dinamically.
               If there are many handlers for a single event, use $rootScope.broadcast().
            */
            var proxy = this.conn.createHubProxy(hubName);
            eventHandlers.forEach((i) => {
                proxy.on(i.eventName,
                    /* hubConnection.received wraps arguments in an array: $(proxy).triggerHandler(makeEventName(eventName), [data.Args]);
                       Here we declare "...args: any[]" and TypeScript wraps arguments in an array in the compiled JavaScript code.
                       As a result, we apparently get an array of arguments in the handler. 
                    */
                    (...args: any[]) => {
                        i.handler(args);
                        this.$rootScope.$applyAsync();
                    }
                    );
            });
        };

        // Be carefull with declaring "...args: any[]". It wraps arguments in an array in the compiled JavaScript code.
        invoke = (hubName: string, methodName: string, ...args: any[]) => {
            if (this.started) {
                var proxy: HubProxy = this.conn.proxies[hubName] || this.conn.createHubProxy(hubName);
                switch (args.length) {
                    case 0:
                        return proxy.invoke(methodName);
                    case 1:
                        return proxy.invoke(methodName, args[0]);
                    case 2:
                        return proxy.invoke(methodName, args[0], args[1]);
                    case 0:
                        return proxy.invoke(methodName, args[0], args[1], args[2]);
                }
            }
        }

    } // end of class SignalRService

    //export function useRouteTitle(app: ng.IModule) {
    //    app.run([ngNames.$rootScope, function ($rootScope: IAppRootScopeService) {
    //        $rootScope.$on('$routeChangeSuccess', (event, current, previous) => {
    //            $rootScope.pageTitle = current.title;
    //        });
    //    }]);
    //};

    export class StateTitleSyncer {
        static $inject = [ngNames.$rootScope];
        constructor($rootScope: IAppRootScopeService) {
            $rootScope.$on('$stateChangeSuccess',(event, toState, toParams, fromState, fromParams) => {
                $rootScope.pageTitle = toState.data && toState.data.title;
                $rootScope.secondaryTitle = toState.data && toState.data.secondaryTitle;
                //window.document.title = current.title + ' \u002d Bla bla bla'; // &ndash;
            });
        }
    };

    export class WrongClockDetector {
        static $inject = [ngNames.$http, ngNames.$rootScope];
        constructor($http: ng.IHttpService, $rootScope: IAppRootScopeService) {
            ngHttpGet($http,
                app.sessionsApiUrl('millis_since_epoch'),
                null,
                (data) => {
                    $rootScope.isClockWrong = (Math.abs(Date.now() - data) > 120000); // Calculate the difference between the client and the provided time
                }
                );
        }
    };

    export class HrefWhitelistConfig {
        static $inject = [app.ngNames.$compileProvider];
        constructor($compileProvider: ng.ICompileProvider) {
            // /^(?:https?:)?\/\/englm\.blob\.core\.windows\.net\/|^https?:\/\/(?:dev\w\.)?englisharium\.com\/|^skype:/
            $compileProvider.aHrefSanitizationWhitelist(/^skype:/);
        }
    }

    export class SceWhitelistConfig {
        static $inject = [app.ngNames.$sceDelegateProvider];
        constructor($sceDelegateProvider: ng.ISCEDelegateProvider) {
            $sceDelegateProvider.resourceUrlWhitelist(['self', 'http*://' + app.BlobDomainName + '/**']);
        }
    }

    export function anticacher() {
        // The actual string length is not that much important. If a proxy server sees a '?' in the url, it will not cache
        // Date.now() gives repeatable values if called from a timer set by increments of ten seconds.
        return app.formatFixedLength(Math.floor(Math.random() * 10000), 4);
    };

    // $http does not send data in body in a GET request, it sends only URL params.
    export function ngHttpGet($http: ng.IHttpService, urlPath: string, params: any, successCallback: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        var ps = params || {};
        ps._ = anticacher();
        return $http.get(urlPath, { params: ps })
            .success(successCallback || angular.noop)
            .error(app.logError)
            .finally(finallyCallback || angular.noop);
    };

    export function ngHttpPost($http: ng.IHttpService, url: string, data: any, successCallback?: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        return $http.post(url, data)
            .success(successCallback || angular.noop)
            .error(app.logError)
            .finally(finallyCallback || angular.noop);
    };

    /* How to post a form-encoded, not JSON-encoded, data */
    //    $http({
    //        method: 'POST',
    //        url: app.loginUrl(),
    //        data: {
    //            userName: userName,
    //        },
    //        /* The OWIN authintication middleware does not accept JSON. It wants a form. */
    //        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
    //        transformRequest: function (obj) {
    //            var str = [];
    //            for (var p in obj)
    //                str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
    //            return str.join("&");
    //        }
    //    })

    export function ngHttpPut($http: ng.IHttpService, url: string, data: any, successCallback?: ng.IHttpPromiseCallback<any>, finallyCallback?: () => any) {
        return $http.put(url, data)
            .success(successCallback || angular.noop)
            .error(app.logError)
            .finally(finallyCallback || angular.noop);
    };

    export function getUserPresentation($http: ng.IHttpService, id: number, successCallback: (data: string) => void) {
        var callback = (data: any) => { (successCallback || angular.noop)(data || ''); }; // The empty string means there has been a request and there is definitly no presentation text.
        $http.get(
            // Encourage caching.   
            //app.accountsApiUrl('presentation/' + id), { cache: true }
            app.getBlobUrl('user-presentations', intToKey(id)), { cache: true }
            )
            .success(callback)
            .error(() => callback(null)); // Azure Blob can return 404 Not Found.
    }

    // We inject the $filter service to be able to call the standard filter internally.
    export function AppDateFilter($filter) {
        return (date: Date) => {
            var f = $filter('date'); // The standard filter.
            return angular.isDefined(date) ? (f(date, 'fullDate') + ' ' + f(date, 'shortTime')) : null;
        }
    };

    export function AppDateTimeFilter() {
        return (date: Date) => {
            return angular.isDefined(date) ? moment(date).format(app.DateTimeFormat) : null;
        }
    };

    // Format exercise length from milliseconds to min:sec
    export function AppMsecToMinSecFilter() {
        return (valMsec: number) => {
            return app.formatMsec(valMsec);
        }
    };

    // Format exercise length depending on the exercise type. app.IReview has exerciseLength and exerciseType.
    export function AppLengthFilter() {
        return (length: number, type: string) => {
            return ExerciseType.formatLength(length, type);
        }
    };

    // Format date like 'today', 'yesterday', 'last monday'
    export function AppDateHumanFilter() {
        return (date: any) => {
            return app.formatDateHuman(date);
        }
    };

    // Format date like '45 minutes', '22 hours'
    export function AppDateAgoFilter() {
        return (date: any) => {
            return app.formatDateAgo(date);
        }
    };

    export function AvatarSmallFilter() {
        return (id: number) => {
            return app.getAvatarSmallUrl(id);
        }
    };

    export function AvatarLargeFilter() {
        return (id: number) => {
            return app.getAvatarLargeUrl(id);
        }
    };

    angular.module(app.utilsNg, [])
        .filter('appDate', [app.ngNames.$filter, app.AppDateFilter])
        .filter('appDateTime', [app.AppDateTimeFilter])
        .filter('appMsecToMinSec', [app.AppMsecToMinSecFilter])
        .filter('appLength', [app.AppLengthFilter])
        .filter('appDateHuman', [app.AppDateHumanFilter])
        .filter('appDateAgo', [app.AppDateAgoFilter])
        .filter('appAvatarSmall', [app.AvatarSmallFilter])
        .filter('appAvatarLarge', [app.AvatarLargeFilter])
        .config(app.SceWhitelistConfig)
        .controller('Help', app.Help);
    ;

} // end of module app


