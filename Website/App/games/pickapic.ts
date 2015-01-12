module app.games {

    export class Pickapic {

        query: string;
        collectionName: string;
        controlNumber: number;
        urls: string[];
        groupSizes: number[] = [3, 6, 9];
        groupSize: number = 3;
        totalCount: number = 0;
        group: number = null; // Pagination: First page is 1.
        shifts: number[];
        spots: number[];
        results: number[];
        result: number;
        //test: string[];

        static $inject = [app.ngNames.$scope, app.ngNames.$http];

        constructor(
            private $scope: app.IScopeWithViewModel,
            private $http: ng.IHttpService
            ) {
            /* ----- Constructor  ----- */
            $scope.vm = this;
            /* ----- End of constructor  ----- */
        }

        search = () => {
            this.totalCount = 0;
            this.spots = [];

            this.$http.get(
                app.pickapicApiUrl(),
                {
                    params: {
                        q: this.query,
                    },
                    cache: true,
                }
                )
                .success((data: any) => {
                    this.controlNumber = data.controlNumber;
                    this.collectionName = data.collectionName;
                    var urls: any[] = (data && data.urls) || [];
                    var groupCount = Math.floor(urls.length / this.groupSize);
                    this.totalCount = groupCount * this.groupSize;
                    this.urls = urls.slice(0, this.totalCount);
                    this.newSpots();
                })
                .error(app.logError);
        };

        private newSpots = () => {
            this.spots = [];
            if (this.totalCount) {
                for (var i = 0; i < this.groupSize; i++) {
                    this.spots.push(i);
                }
            }
            this.shuffle();
            this.group = 1;
        };

        shuffle = () => {
            this.shifts = []; // if the variable is not an array, angular.copy will not work.          
            angular.copy(this.spots, this.shifts); // Deep copy.
            app.arrShuffle(this.shifts);
            this.results = [];
            this.result = null;
        };

        getUrl = (spot: number) => {
            var start = (this.group - 1) * this.groupSize; // Pagination: First page is 1.
            var idx = start + this.shifts[spot];
            return this.urls ? this.urls[idx] : null;
        };

        imgClick = (spot: number) => {
            var shift = this.shifts[spot];
            var len = this.results.length;
            if ((len > 0) && (this.results[len - 1] === shift)) {
                this.results.pop();
            }
            else
                if (this.results.indexOf(shift) === -1) {
                    this.results.push(shift);
                }

            this.result = this.calcResult();
        };

        isLast = (spot: number) => {
            var shift = this.shifts[spot];
            var len = this.results.length;
            return (len > 0) && (this.results[len - 1] === shift);
        };

        getPosition = (spot: number) => {
            var shift = this.shifts[spot];
            var pos = this.results.indexOf(shift) + 1;
            return pos || null;
        };

        isClickable = (spot: number) => {
            return (this.getPosition(spot) == null) || this.isLast(spot);
        };

        // +http://en.wikipedia.org/wiki/NATO_phonetic_alphabet
        calcResult = () => {
            if (this.results.length !== this.groupSize) {
                return null;
            }

            // Our numbers in this.results are base3 or base6 or base9, not base10. We produce a compacted base10 number.
            var num = this.results.reduce(
                (previous: number, value: number, index: number) => {
                    return previous + value * Math.pow(this.groupSize, index);
                },
                0);
            return num;

            // We use two bases to convert the number. We alternate the 'digit' and the 'letter' base. The goal is the result string consists of alternating digits and letters. The prototipe is Canadian Postal Code, e.g. M6S2L4.
            //var chars1 = '23456789'; // 8 chars. Exclude 1, 0.
            //var chars2 = 'abcdefghjklmnpqrstuvwxyz'; // 24 chars. Exclude l, o.
            //var radix1 = chars1.length;
            //var radix2 = chars2.length;
            //var flag = true;
            //var res = '';
            //do {
            //    var radix = flag ? radix1 : radix2;
            //    var chars = flag ? chars1 : chars2;
            //    res = res + chars[num % radix];
            //    num = Math.floor(num / radix);
            //    flag = !flag;
            //} while (num != 0);
            //return res;
        }

    } // end of class Ctrl

    angular.module(app.myAppName, [app.utilsNg, 'ui.bootstrap', 'angular-loading-bar'])
        .controller('Pickapic', Pickapic);

} // end of module app.games_pick

