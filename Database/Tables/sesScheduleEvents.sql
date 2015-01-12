CREATE TABLE [dbo].[sesScheduleEvents] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserId]    INT            NOT NULL,
    [Start]     SMALLDATETIME  NOT NULL,
    [End]       SMALLDATETIME  NOT NULL,
    [Type]      CHAR (6)       NOT NULL,
    [Attribute] NVARCHAR (100) NULL,
    CONSTRAINT [PK_sesScheduleEvents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_appScheduleItems_StartEnd] CHECK ([Start]<[End]),
    CONSTRAINT [CK_sesScheduleEvents_Type] CHECK (substring([Type],(1),(2))='SE'),
    CONSTRAINT [FK_sesScheduleEvents_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_sesScheduleEvents_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Attribute]
    ON [dbo].[sesScheduleEvents]([Attribute] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserId_Start]
    ON [dbo].[sesScheduleEvents]([UserId] ASC, [Start] ASC);


GO
GRANT SELECT
    ON OBJECT::[dbo].[sesScheduleEvents] TO [websiterole]
    AS [dbo];

