module app.library {

    export class History extends app.CtrlBase {

        private static ShortDayFormat = 'YYMMDD';
        private static LongDayFormat = 'YYYYMMDD';
        private days: string[];
        date: Date;
        minDate: Date;
        maxDate: Date;
        titleDate: Date;

        static $inject = [app.ngNames.$scope, app.ngNames.$http, app.ngNames.$rootScope, app.ngNames.$window];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: angular.IHttpService,
            private $rootScope: angular.IRootScopeService,
            private $window: angular.IWindowService
            ) {
            /* ----- Constructor  ----- */
            super($scope);

            // The days when history items occured are passed with the page.
            this.days = app['daysParam'] || [];

            // Setup the date diapason for the calendar. It will ask only about these dates in dateDisabled().
            var minDate = this.days[0] || moment().format(History.ShortDayFormat);
            var maxDate = minDate;
            this.days.forEach((i) => {
                if (minDate.localeCompare(i) > 0) {
                    minDate = i;
                }
                if (maxDate.localeCompare(i) < 0) {
                    maxDate = i;
                }
            });
            this.minDate = moment(minDate, History.ShortDayFormat).toDate();
            this.maxDate = moment(maxDate, History.ShortDayFormat).toDate();

            /* ----- End of constructor  ----- */
        }

        private clear = () => {
            this.pgReset();
            this.titleDate = null;
            this.$rootScope.$broadcast(app.library.ResourceList.Clear);
        };

        dateDisabled = (date, mode) => {
            // Datepicker passes date in dateDisabled as noon, not midnight, to prevent repeated dates due to a bug with timezones. 
            var day = moment(date).startOf('day').format(History.ShortDayFormat);
            // We expect mode is always "day". It is set up by the max-mode attribute.
            var enabled = (mode === 'day') && this.days.some((i) => { return i === day; });
            return !enabled;
        }

        pgLoad = () => {
            this.clear();
            if (!this.busy) {
                this.busy = true;
                var date = this.date;

                app.ngHttpGet(this.$http,
                    app.libraryApiUrl('day_history'),
                    {
                        offset: this.pgOffset(),
                        limit: this.pgLimit,
                        day: app.encodeLocalTime(moment(date).startOf('day').toDate()),
                    },
                    (data) => {
                        if (data && angular.isArray(data.items)) {
                            // Display the view time in the title
                            data.items.forEach((i) => {
                                var parts = i.localTime.split('/');
                                i.title = app.formatFixedLength(parts[3], 2) + ':' + app.formatFixedLength(parts[4], 2) + '\xA0\xA0' + i.title;
                            });
                            this.$rootScope.$broadcast(ResourceList.Display, { resources: data.items });
                            this.pgTotal = data.totalCount;
                            this.titleDate = date;
                        }
                    },
                    () => { this.busy = false; }
                    );
            }
        };

        private processResources = (items: IResource[]) => {
            items.forEach((i) => {
                var parts = i.localTime.split('/');
                i.title = app.formatFixedLength(parts[3], 2) + ':' + app.formatFixedLength(parts[4], 2) + '\xA0\xA0' + i.title;
            });
        };

    } // end of class History

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('History', History)
        .controller('ResourceList', ResourceList)
    ;

} // end of module app.library
