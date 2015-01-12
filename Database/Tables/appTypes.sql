CREATE TABLE [dbo].[appTypes] (
    [Id]            CHAR (6)        NOT NULL,
    [Name]          NVARCHAR (100)  NULL,
    [Description]   NVARCHAR (1000) NULL,
    [AttributeType] CHAR (6)        NULL,
    CONSTRAINT [PK_appTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_appTypes_appTypes] FOREIGN KEY ([AttributeType]) REFERENCES [dbo].[appTypes] ([Id])
);









