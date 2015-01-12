CREATE TABLE [dbo].[friFriends] (
    [UserId]          INT            NOT NULL,
    [FriendUserId]    INT            NOT NULL,
    [IsActive]        BIT            NOT NULL,
    [LastContactDate] SMALLDATETIME  CONSTRAINT [DF_relRelationships_Date] DEFAULT (sysutcdatetime()) NULL,
    [LastContactType] CHAR (6)       NULL,
    [RecordingRate]   DECIMAL (9, 2) NULL,
    [WritingRate]     DECIMAL (9, 2) NULL,
    [SessionRate]     DECIMAL (9, 2) NULL,
    CONSTRAINT [PK_friFriends] PRIMARY KEY CLUSTERED ([UserId] ASC, [FriendUserId] ASC),
    CONSTRAINT [CK_friFriends_LastContactType] CHECK (substring([LastContactType],(1),(2))='CN'),
    CONSTRAINT [FK_friFriends_appTypes] FOREIGN KEY ([LastContactType]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_friFriends_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_friFriends_appUsers_Friend] FOREIGN KEY ([FriendUserId]) REFERENCES [dbo].[appUsers] ([Id])
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IXC_FriendUserId_UserId]
    ON [dbo].[friFriends]([FriendUserId] ASC, [UserId] ASC)
    INCLUDE([IsActive], [RecordingRate]);





