CREATE TABLE [dbo].[relMessageTypes] (
    [Id]            NCHAR (4)      NOT NULL,
    [Description]   NVARCHAR (100) NOT NULL,
    [AttributeType] NCHAR (4)      NULL,
    CONSTRAINT [PK_relMessageTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_relMessageTypes_relMessageAttributeTypes] FOREIGN KEY ([AttributeType]) REFERENCES [dbo].[relMessageAttributeTypes] ([Id])
);

