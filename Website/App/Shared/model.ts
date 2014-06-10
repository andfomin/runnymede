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

    export interface IExercise2 {
        id: number;
        createTime: string;
        typeId: string;
        artefactId: string;
        title: string;
        length: number;
        reviews: IReview2[];
    }

    export interface IReview2 {
        id: number;
        exerciseId: number;
        requestTime: Date;
        cancelTime: Date;
        startTime: Date;
        finishTime: Date;
        authorName: string;
        reviewerName: string;
        reward: number;
        exerciseLength: number;
        comment: string;
        suggestions: ISuggestion[];
    }

    export interface IRemark2 {
        reviewId: number;
        creationTime: number; // The difference in milliseconds between the remark creation time and the review start time.
        start: number;
        finish: number;
        text: string;
        keywords: string;
        dirtyTime: Date;
    }

    export interface ISuggestion {
        reviewId: number;
        creationTime: number; // The difference in milliseconds between the item creation time and the review start time.
        text: string;
        dirtyTime: Date;
    }

    export class Review2 implements IReview2 {
        id: number;
        exerciseId: number;
        requestTime: Date;
        cancelTime: Date;
        startTime: Date;
        finishTime: Date;
        authorName: string;
        reviewerName: string;
        reward: number;
        exerciseLength: number;
        comment: string;
        suggestions: ISuggestion[];

        constructor(data: any) {
            this.id = data.id;
            this.exerciseId = data.exerciseId;
            this.requestTime = new Date(data.requestTime);
            this.cancelTime = data.cancelTime ? new Date(data.cancelTime) : null;
            this.startTime = data.startTime ? new Date(data.startTime) : null;
            this.finishTime = data.finishTime ? new Date(data.finishTime) : null;
            this.authorName = data.authorName;
            this.reviewerName = data.reviewerName;
            this.reward = data.reward;
            this.exerciseLength = data.exerciseLength;
            this.comment = data.comment;
            this.suggestions = data.suggestions;
        } // end of ctor
    }

}
