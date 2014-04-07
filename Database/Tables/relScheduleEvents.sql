CREATE TABLE [dbo].[relScheduleEvents] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [Start]            SMALLDATETIME   NOT NULL,
    [End]              SMALLDATETIME   NOT NULL,
    [Type]             NCHAR (4)       NOT NULL,
    [UserId]           INT             NOT NULL,
    [SecondUserId]     INT             NULL,
    [Price]            DECIMAL (18, 2) NULL,
    [CreationTime]     DATETIME2 (2)   NOT NULL,
    [ConfirmationTime] DATETIME2 (2)   NULL,
    [CancellationTime] DATETIME2 (2)   NULL,
    [ClosingTime]      DATETIME2 (2)   NULL,
    CONSTRAINT [PK_relScheduleEvents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_relScheduleEvents] CHECK ([Start]<[End]),
    CONSTRAINT [FK_relScheduleEvents_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_relScheduleEvents_relScheduleEventTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[relScheduleEventTypes] ([Id])
);




















GO
CREATE NONCLUSTERED INDEX [IX_UserId_Start_Covering]
    ON [dbo].[relScheduleEvents]([UserId] ASC, [Start] DESC)
    INCLUDE([End], [Type]);




GO
GRANT SELECT
    ON OBJECT::[dbo].[relScheduleEvents] TO [websiterole]
    AS [dbo];


GO


