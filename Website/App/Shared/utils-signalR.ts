module app {

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
            private $rootScope: angular.IRootScopeService
            )
        /* ----- Constructor  ----- */ {
            // ConnectionId is keept the same on reconnects.
            this.conn = this.$.hubConnection();
            if (app.isDevHost) {
                this.conn.logging = true;
            }
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

} // end of module app
