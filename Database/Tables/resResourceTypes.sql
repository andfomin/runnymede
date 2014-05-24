CREATE TABLE [dbo].[resResourceTypes] (
    [Id]   NCHAR (4)      NOT NULL,
    [Name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_resResourceTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[resResourceTypes] TO [websiterole]
    AS [dbo];

