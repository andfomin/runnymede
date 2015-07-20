CREATE TABLE [dbo].[exeReviews] (
    [Id]          INT           NOT NULL,
    [ExerciseId]  INT           NOT NULL,
    [UserId]      INT           NULL,
    [RequestTime] DATETIME2 (2) CONSTRAINT [DF_exeReviews_RequestTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [StartTime]   DATETIME2 (2) NULL,
    [FinishTime]  DATETIME2 (2) NULL,
    CONSTRAINT [PK_exeReviews] PRIMARY KEY CLUSTERED ([Id] ASC)
);




























































GO
GRANT SELECT
    ON OBJECT::[dbo].[exeReviews] TO [websiterole]
    AS [dbo];


GO
CREATE NONCLUSTERED INDEX [IX_UserId_FinishTime]
    ON [dbo].[exeReviews]([UserId] ASC, [FinishTime] ASC);

