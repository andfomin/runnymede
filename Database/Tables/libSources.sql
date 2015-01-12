CREATE TABLE [dbo].[libSources] (
    [Id]       NCHAR (4)       NOT NULL,
    [Name]     NVARCHAR (100)  NOT NULL,
    [HomePage] NVARCHAR (2000) NULL,
    [IconUrl]  NVARCHAR (2000) NULL,
    CONSTRAINT [PK_libSources] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO


