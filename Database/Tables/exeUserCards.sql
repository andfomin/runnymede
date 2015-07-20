CREATE TABLE [dbo].[exeUserCards] (
    [UserId]      INT              NOT NULL,
    [ServiceType] CHAR (6)         NOT NULL,
    [CardId]      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_exeUserCards] PRIMARY KEY CLUSTERED ([UserId] ASC, [ServiceType] ASC),
    CONSTRAINT [CK_exeUserCards] CHECK (substring([ServiceType],(1),(2))='SV'),
    CONSTRAINT [FK_exeUserCards_appTypes] FOREIGN KEY ([ServiceType]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_exeUserCards_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeUserCards_exeCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[exeCards] ([Id])
);






GO
GRANT UPDATE
    ON OBJECT::[dbo].[exeUserCards] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[exeUserCards] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[exeUserCards] TO [websiterole]
    AS [dbo];

