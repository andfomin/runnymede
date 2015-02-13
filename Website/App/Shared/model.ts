module app {

    export interface IUser {
        id: number;
        displayName: string;
        userName: string;
        skypeName: string;
        isTeacher: boolean;
        recordingRate: number;
        writingRate: number;
        sessionRate: number;
        announcement: string;
        presentation: string;
        phoneNumber: string;
        phoneNumberConfirmed: boolean;
        email: string;
        emailConfirmed: boolean;
    };

    export interface IExercise {
        id: number;
        createTime: string;
        type: string;
        artifact: string;
        title: string;
        length: number;
        reviews: IReview[];
    };

    export interface IReview {
        id: number;
        exerciseId: number;
        requestTime: string; // Date;
        cancelationTime: string; // Date;
        startTime: string; // Date;
        finishTime: string; // Date;
        authorName: string;
        userId: number;
        reviewerName: string;
        price: number;
        exerciseLength: number;
        comment: IComment;
        suggestions: ISuggestion[];
    };

    // Review piece, the base interface for IRemark, ISuggestion, IComment.
    export interface IPiece {
        reviewId: number;
        type: string; // Constants are declared in CtrlBase
        //creationTime: number; // The difference in milliseconds between the item creation time and the review start time.
        id: number;
        dirtyTime: Date; // Invalidate the item.
    };

    export interface IRemark extends IPiece {
        start: number;
        finish: number;
        correction: string;
        comment: string;
        // Writing photo
        page: number; // first page is 1
        x: number;
        y: number;
        like: boolean;
    };

    export interface ISuggestion extends IPiece {
        suggestion: string;
        keywords: string;
        categoryId: string;
    };

    export interface IComment extends IPiece {
        comment: string;
    };

    export interface IScheduleEvent {
        id: number;
        start: moment.Moment;
        end: moment.Moment;
        type: string;
        userId: number;
        // These properties are supported by FullCalendar
        title: string;
        url: string; // Chrome weiredly tries to silently send a request to this URL as if it was a real URL. It gets an unrecoverable error for 'javascript:;' and the event fials to render. If  the value is malformed, Firefox uncoditionally goes to the URL on click and reports the unknown format to the user.
        color: string;
        backgroundColor: string;
        borderColor: string;
        className: string[];
    };

    export interface ISession {
        id: number;
        start: string;
        end: string;
        hostUserId: number;
        guestUserId: number;
        price: number;
        requestTime: string;
        confirmationTime: string;
        cancellationTime: string;
        cancellationUserId: number;
        disputedTime: string;
        disputeUser: number;
        finishTime: string;
    };

    export interface IMessage {
        id: number;
        type: string;
        postTime: string;
        receiveTime: string;
        senderUserId: number;
        senderDisplayName: string;
        recipientUserId: number;
        recipientDisplayName: string;
        extId: string;
        text: string;
    };

    export interface IResource {
        id: number;
        url: string; // Entered by the user while adding manually
        naturalKey: string; // Stored in database and serach
        format: string;
        segment: string;
        title: string;
        categoryIds: string;
        tags: string;
        source: string;
        hasExplanation: boolean;
        hasExample: boolean;
        hasExercise: boolean;
        hasText: boolean;
        hasPicture: boolean;
        hasAudio: boolean;
        hasVideo: boolean;
        isPersonal: boolean;
        languageLevelRating: number;
        priority: number; // Copycat segment priority, affects display order. Values are from 0 to 4.
        comment: string;
        // Not persisted.
        viewed: boolean;
        localTime: string; // for History
    }

}
