CREATE TABLE [dbo].[relSkypeDirectory] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [UserId]       INT            NULL,
    [Skype]        NVARCHAR (100) NOT NULL,
    [TimeBegin]    DATETIME2 (7)  CONSTRAINT [DF_relLearnersOnSkype_TimeBegin] DEFAULT (sysutcdatetime()) NOT NULL,
    [TimeEnd]      DATETIME2 (7)  NULL,
    [Announcement] NVARCHAR (200) NULL,
    CONSTRAINT [PK_relSkypeAvailability] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_relLearnersOnSkype_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id])
);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[relSkypeDirectory] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[relSkypeDirectory] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[relSkypeDirectory] TO [websiterole]
    AS [dbo];

