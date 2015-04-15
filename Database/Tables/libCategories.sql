CREATE TABLE [dbo].[libCategories] (
    [Id]          NCHAR (4)      NOT NULL,
    [ParentId]    NCHAR (4)      NULL,
    [Name]        NVARCHAR (200) NOT NULL,
    [Position]    SMALLINT       NULL,
    [FeatureCode] NVARCHAR (4)   NULL,
    [Display]     BIT            CONSTRAINT [DF_libCategories_Display] DEFAULT ((1)) NOT NULL,
    [NamePath]    NVARCHAR (500) NULL,
    [IdPath]      NVARCHAR (50)  NULL,
    CONSTRAINT [PK_libCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_libCategories_libCategories] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[libCategories] ([Id])
);














GO



GO


