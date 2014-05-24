CREATE TABLE [dbo].[resResources] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [Url]       NVARCHAR (2000) NOT NULL,
    [Title]     NVARCHAR (1000) NULL,
    [Type]      NCHAR (4)       NULL,
    [EntryDate] SMALLDATETIME   CONSTRAINT [DF_resResources_EntryDate] DEFAULT (getutcdate()) NOT NULL,
    [Keywords]  NVARCHAR (1000) NULL,
    [BrandId]   INT             NULL,
    [MediaType] NCHAR (4)       NULL,
    CONSTRAINT [PK_resResources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_resResources_resBrands] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[resBrands] ([Id]),
    CONSTRAINT [FK_resResources_resMediaTypes] FOREIGN KEY ([MediaType]) REFERENCES [dbo].[resMediaTypes] ([Id]),
    CONSTRAINT [FK_resResources_resResourceTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[resResourceTypes] ([Id])
);




GO
GRANT UPDATE
    ON OBJECT::[dbo].[resResources] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[resResources] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[resResources] TO [websiterole]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[resResources] TO [websiterole]
    AS [dbo];

