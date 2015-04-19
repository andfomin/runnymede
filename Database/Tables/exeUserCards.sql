CREATE TABLE [dbo].[exeUserCards] (
    [UserId] INT      NOT NULL,
    [Type]   CHAR (6) NOT NULL,
    [CardId] INT      NOT NULL,
    CONSTRAINT [PK_exeUserCards_1] PRIMARY KEY CLUSTERED ([UserId] ASC, [Type] ASC),
    CONSTRAINT [FK_exeUserCards_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
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

