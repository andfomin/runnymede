CREATE TABLE [dbo].[libExponents] (
    [CategoryId]     NCHAR (4)       NOT NULL,
    [ReferenceLevel] NCHAR (2)       NOT NULL,
    [Text]           NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_libExponents] PRIMARY KEY CLUSTERED ([CategoryId] ASC, [ReferenceLevel] ASC),
    CONSTRAINT [FK_libExponents_libCategories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[libCategories] ([Id])
);








GO
GRANT SELECT
    ON OBJECT::[dbo].[libExponents] TO [websiterole]
    AS [dbo];

