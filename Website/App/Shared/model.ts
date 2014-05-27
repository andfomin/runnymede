interface App {
    // Declared in bceg-lookup.js
    bcegLookup: any;
}

module App.Model {

    export interface IUser {
        id: number;
        displayName: string;
        userName: string;
        skype: string;
        isTeacher: boolean;
        isAssistant: boolean;
        timezoneName: string;
        reviewRate: number;
        sessionRate: number;
        announcement: string;
        avatarLargeUrl: string;
        avatarSmallUrl: string;
        phoneNumber: string;
        phoneNumberConfirmed: boolean;
        email: string;
        emailConfirmed: boolean;
    }

    export interface IExercise {
        id: number;
        createTime: string;
        typeId: string;
        artefactId: string;
        title: KnockoutObservable<string>;
        length: number;
        reviews: KnockoutObservableArray<IReview>;
        // Non-persisted
        isTitleDirty: any;
        formattedLength: string;
    }

    export interface IExercise2 {
        id: number;
        createTime: string;
        typeId: string;
        artefactId: string;
        title: string;
        length: number;
        reviews: IReview2[];
    }

    export interface IReview {
        id: number;
        exerciseId: number;
        requestTime: string;
        cancelTime: KnockoutObservable<string>;
        startTime: KnockoutObservable<string>;
        finishTime: KnockoutObservable<string>;
        authorName: string;
        //reward: number;
        // Non-persisted
        formattedReward: string;
    }

    export interface IReview2 {
        id: number;
        exerciseId: number;
        requestTime: Date;
        cancelTime: Date;
        startTime: Date;
        finishTime: Date;
        authorName: string;
        reward: number;
    }

    export interface IRemark {
        id: string;
        reviewId: number;
        start: KnockoutObservable<number>;
        finish: KnockoutObservable<number>;
        tags: KnockoutObservable<string>;
        text: KnockoutObservable<string>;
        starred: KnockoutObservable<boolean>;
        // Non-persisted
        dirtyFlag: any;
        formatStart: KnockoutComputed<string>;
        formatFinish: KnockoutComputed<string>;
    }

    export interface IRemark2 {
        id: string;
        reviewId: number;
        start: number;
        finish: number;
        tags: string;
        text: string;
        dirty: boolean;
    }

    export class Exercise implements IExercise {

        id: number;
        createTime: string;
        typeId: string;
        artefactId: string;
        title: KnockoutObservable<string>;
        length: number;
        reviews: KnockoutObservableArray<Review>;
        // Non-persisted
        isTitleDirty: any;
        formattedLength: string;

        constructor(data: any) {
            this.id = data.id;
            this.typeId = data.typeId;
            this.artefactId = data.artefactId;
            this.length = data.length;
            this.title = ko.observable(data.title);
            this.createTime = App.Utils.formatDateLocal(data.createTime);
            this.reviews = ko.observableArray(<Review[]>$.map(data.reviews || [], i => new Review(i)));
            this.formattedLength = App.Utils.formatMsec(this.length);

            //var titles = $.map(this.reviews(), (i) => { return i.note; });
            //titles.push(this.title);
            this.isTitleDirty = new ko.DirtyFlag([this.title], false);
        } // end of ctor

        viewUrl() {
            return App.Utils.reviewsUrl('view/' + this.id);
        }
    } // end of class

    export class Review implements IReview {
        id: number;
        exerciseId: number;
        requestTime: string;
        cancelTime: KnockoutObservable<string>;
        startTime: KnockoutObservable<string>;
        finishTime: KnockoutObservable<string>;
        authorName: string;
        // Non-persisted
        formattedReward: string;

        constructor(data: any) {
            this.id = data.id;
            this.exerciseId = data.exerciseId;
            this.authorName = data.authorName;
            this.formattedReward = App.Utils.formatMoney(data.reward);
            this.requestTime = App.Utils.formatDateLocal(data.requestTime);
            this.cancelTime = ko.observable(App.Utils.formatDateLocal(data.cancelTime));
            this.startTime = ko.observable(App.Utils.formatDateLocal(data.startTime));
            this.finishTime = ko.observable(App.Utils.formatDateLocal(data.finishTime));
        } // end of ctor

        viewUrl() {
            return this.composeUrl('view');
        }

        editUrl() {
            return this.composeUrl('edit');
        }

        private composeUrl(action) {
            return App.Utils.reviewsUrl(action + '/' + this.id);
        }

        status() {
            if (this.finishTime()) {
                return 'Finished ' + this.finishTime();
            }
            else {
                if (this.startTime()) {
                    return 'Started ' + this.startTime();
                }
                else {
                    if (this.cancelTime()) {
                        return 'Canceled ' + this.cancelTime();
                    }
                    else {
                        return 'Requested ' + this.requestTime;
                    }
                }
            }
        }

    } // end of class

    export class Remark implements IRemark {
        id: string;
        reviewId: number;
        start: KnockoutObservable<number>;
        finish: KnockoutObservable<number>;
        tags: KnockoutObservable<string>;
        text: KnockoutObservable<string>;
        starred: KnockoutObservable<boolean>;
        // Non-persisted
        dirtyFlag: any;
        formatStart: KnockoutComputed<string>;
        formatFinish: KnockoutComputed<string>;

        constructor(data: any) {
            this.reviewId = data.reviewId;
            this.id = data.id;
            this.start = ko.observable(data.start);
            this.finish = ko.observable(data.finish);
            this.tags = ko.observable(data.tags);
            this.text = ko.observable(data.text);
            this.starred = ko.observable(data.starred);

            var trackArr: any[] = [this.start, this.finish, this.tags, this.text, this.starred];
            this.dirtyFlag = new ko.DirtyFlag(trackArr, true);

            this.formatStart = ko.computed(() => {
                return App.Utils.formatMsec(this.start());
            });

            this.formatFinish = ko.computed(() => {
                return App.Utils.formatMsec(this.finish());
            });
        } // end of ctor

        tagsUrl() {
            //return 'https://' + window.location.host + App.Utils.reviewsUrl('tagsearch?q=' + encodeURIComponent(this.tags()));
            return App.Utils.reviewsUrl('tag-search?q=' + encodeURIComponent(this.tags()));
        }

        tagAliases() {
            var tags = $.map(this.tags().split(','), i => $.trim(i));
            var aliases = $.map(tags, i => App['bcegLookup'][i] || i);
            return aliases.join(', ');
        }

    } // end of class

}
