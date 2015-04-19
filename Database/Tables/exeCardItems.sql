CREATE TABLE [dbo].[exeCardItems] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [CardId]   INT            NOT NULL,
    [Position] NVARCHAR (10)  NOT NULL,
    [Contents] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_exeCardItems_1] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeCardItems_exeCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[exeCards] ([Id])
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[exeCardItems] TO [websiterole]
    AS [dbo];

