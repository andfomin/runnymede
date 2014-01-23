var App;
(function (App) {
    (function (Model) {
        var Exercise = (function () {
            function Exercise(data) {
                this.id = data.id;
                this.typeId = data.typeId;
                this.artefactId = data.artefactId;
                this.length = data.length;
                this.title = ko.observable(data.title);
                this.createTime = App.Utils.formatDateLocal(data.createTime);
                this.reviews = ko.observableArray($.map(data.reviews || [], function (i) {
                    return new Review(i);
                }));
                this.formattedLength = App.Utils.formatMsec(this.length);

                //var titles = $.map(this.reviews(), (i) => { return i.note; });
                //titles.push(this.title);
                this.isTitleDirty = new ko.DirtyFlag([this.title], false);
            }
            Exercise.prototype.viewUrl = function () {
                return App.Utils.reviewsUrl('view/' + this.id);
            };
            return Exercise;
        })();
        Model.Exercise = Exercise;

        var Review = (function () {
            function Review(data) {
                this.id = data.id;
                this.exerciseId = data.exerciseId;
                this.authorName = data.authorName;
                this.formattedReward = App.Utils.formatMoney(data.reward);
                this.requestTime = App.Utils.formatDateLocal(data.requestTime);
                this.cancelTime = ko.observable(App.Utils.formatDateLocal(data.cancelTime));
                this.startTime = ko.observable(App.Utils.formatDateLocal(data.startTime));
                this.finishTime = ko.observable(App.Utils.formatDateLocal(data.finishTime));
            }
            Review.prototype.viewUrl = function () {
                return this.composeUrl('view');
            };

            Review.prototype.editUrl = function () {
                return this.composeUrl('edit');
            };

            Review.prototype.composeUrl = function (action) {
                return App.Utils.reviewsUrl(action + '/' + this.id);
            };

            Review.prototype.status = function () {
                if (this.finishTime()) {
                    return 'Finished ' + this.finishTime();
                } else {
                    if (this.startTime()) {
                        return 'Started ' + this.startTime();
                    } else {
                        if (this.cancelTime()) {
                            return 'Canceled ' + this.cancelTime();
                        } else {
                            return 'Requested ' + this.requestTime;
                        }
                    }
                }
            };
            return Review;
        })();
        Model.Review = Review;

        var Remark = (function () {
            function Remark(data) {
                var _this = this;
                this.reviewId = data.reviewId;
                this.id = data.id;
                this.start = ko.observable(data.start);
                this.finish = ko.observable(data.finish);
                this.tags = ko.observable(data.tags);
                this.text = ko.observable(data.text);
                this.starred = ko.observable(data.starred);

                var trackArr = [this.start, this.finish, this.tags, this.text, this.starred];
                this.dirtyFlag = new ko.DirtyFlag(trackArr, true);

                this.formatStart = ko.computed(function () {
                    return App.Utils.formatMsec(_this.start());
                });

                this.formatFinish = ko.computed(function () {
                    return App.Utils.formatMsec(_this.finish());
                });
            }
            Remark.prototype.tagsUrl = function () {
                /* Use HTTPS to avoid the HTTP Referer header on redirect. +http://en.wikipedia.org/wiki/HTTP_referer#cite_note-10 */
                //return 'https://' + window.location.host + App.Utils.reviewsUrl('tagsearch?q=' + encodeURIComponent(this.tags()));
                return App.Utils.reviewsUrl('tag-search?q=' + encodeURIComponent(this.tags()));
            };

            Remark.prototype.tagAliases = function () {
                var tags = $.map(this.tags().split(','), function (i) {
                    return $.trim(i);
                });
                var aliases = $.map(tags, function (i) {
                    return App['bcegLookup'][i] || i;
                });
                return aliases.join(', ');
            };
            return Remark;
        })();
        Model.Remark = Remark;
    })(App.Model || (App.Model = {}));
    var Model = App.Model;
})(App || (App = {}));
//# sourceMappingURL=model.js.map
