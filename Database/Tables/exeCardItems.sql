CREATE TABLE [dbo].[exeCardItems] (
    [CardId]   UNIQUEIDENTIFIER NOT NULL,
    [Position] NVARCHAR (5)     NOT NULL,
    [Contents] NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_exeCardItems] PRIMARY KEY CLUSTERED ([CardId] ASC, [Position] ASC),
    CONSTRAINT [FK_exeCardItems_exeCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[exeCards] ([Id])
);








GO
GRANT SELECT
    ON OBJECT::[dbo].[exeCardItems] TO [websiterole]
    AS [dbo];

