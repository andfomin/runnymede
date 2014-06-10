CREATE TABLE [dbo].[exeReviews] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [ExerciseId]     INT             NOT NULL,
    [UserId]         INT             NULL,
    [Reward]         DECIMAL (18, 2) NOT NULL,
    [RequestTime]    DATETIME2 (0)   CONSTRAINT [DF_exeReviews_RequestTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [CancelTime]     DATETIME2 (0)   NULL,
    [StartTime]      DATETIME2 (0)   NULL,
    [FinishTime]     DATETIME2 (0)   NULL,
    [AuthorName]     NVARCHAR (100)  NOT NULL,
    [ReviewerName]   NVARCHAR (100)  NULL,
    [ExerciseLength] INT             NULL,
    [Comment]        NVARCHAR (4000) NULL,
    CONSTRAINT [PK_exeReviews] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeReviews_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeReviews_exeExercises] FOREIGN KEY ([ExerciseId]) REFERENCES [dbo].[exeExercises] ([Id])
);




























GO
GRANT SELECT
    ON OBJECT::[dbo].[exeReviews] TO [websiterole]
    AS [dbo];

