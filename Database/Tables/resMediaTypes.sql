CREATE TABLE [dbo].[resMediaTypes] (
    [Id]   NCHAR (4)      NOT NULL,
    [Name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_resMediaTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[resMediaTypes] TO [websiterole]
    AS [dbo];

