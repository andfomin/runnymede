CREATE TABLE [dbo].[resBrands] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (100)  NOT NULL,
    [ImageUrl] NVARCHAR (2000) NULL,
    CONSTRAINT [PK_resBrands] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[resBrands] TO [websiterole]
    AS [dbo];

