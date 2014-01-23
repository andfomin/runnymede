CREATE TABLE [dbo].[appConstants] (
    [Name]    NVARCHAR (100)  NOT NULL,
    [Value]   NVARCHAR (MAX)  NULL,
    [Comment] NVARCHAR (1000) NULL,
    CONSTRAINT [PK_Constants] PRIMARY KEY CLUSTERED ([Name] ASC)
);

