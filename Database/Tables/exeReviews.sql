CREATE TABLE [dbo].[exeReviews] (
    [Id]              INT            NOT NULL,
    [ExerciseId]      INT            NOT NULL,
    [UserId]          INT            NULL,
    [Price]           DECIMAL (9, 2) NOT NULL,
    [ExerciseType]    CHAR (6)       NOT NULL,
    [ExerciseLength]  INT            NULL,
    [RequestTime]     DATETIME2 (2)  CONSTRAINT [DF_exeReviews_RequestTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [CancelationTime] DATETIME2 (2)  NULL,
    [StartTime]       DATETIME2 (2)  NULL,
    [FinishTime]      DATETIME2 (2)  NULL,
    [AuthorUserId]    INT            NOT NULL,
    [AuthorName]      NVARCHAR (100) NOT NULL,
    [ReviewerName]    NVARCHAR (999) NULL,
    CONSTRAINT [PK_exeReviews] PRIMARY KEY CLUSTERED ([Id] ASC)
);














































GO
GRANT SELECT
    ON OBJECT::[dbo].[exeReviews] TO [websiterole]
    AS [dbo];


GO
CREATE UNIQUE NONCLUSTERED INDEX [UXF_UserId_FinishTime]
    ON [dbo].[exeReviews]([UserId] ASC) WHERE ([UserId] IS NOT NULL AND [FinishTime] IS NULL);

