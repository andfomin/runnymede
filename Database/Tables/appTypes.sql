CREATE TABLE [dbo].[appTypes] (
    [Id]            CHAR (6)        NOT NULL,
    [Name]          NVARCHAR (200)  NULL,
    [Title]         NVARCHAR (1000) NULL,
    [Description]   NVARCHAR (1000) NULL,
    [AttributeType] CHAR (6)        NULL,
    [Details]       XML             NULL,
    CONSTRAINT [PK_appTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_appTypes_appTypes] FOREIGN KEY ([AttributeType]) REFERENCES [dbo].[appTypes] ([Id])
);















