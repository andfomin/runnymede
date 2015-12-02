CREATE TABLE [dbo].[exeCardItems] (
    [CardId]   UNIQUEIDENTIFIER NOT NULL,
    [Position] NVARCHAR (5)     NOT NULL,
    [Content]  NVARCHAR (MAX)   NOT NULL,
    [PlayFrom] DECIMAL (9, 2)   NULL,
    [PlayTo]   DECIMAL (9, 2)   NULL,
    CONSTRAINT [PK_exeCardItems] PRIMARY KEY CLUSTERED ([CardId] ASC, [Position] ASC),
    CONSTRAINT [FK_exeCardItems_exeCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[exeCards] ([Id])
);










GO
GRANT SELECT
    ON OBJECT::[dbo].[exeCardItems] TO [websiterole]
    AS [dbo];

