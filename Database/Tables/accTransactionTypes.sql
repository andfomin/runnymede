CREATE TABLE [dbo].[accTransactionTypes] (
    [Id]            CHAR(4)       NOT NULL,
    [Description]         NVARCHAR (100) NOT NULL,
    [AttributeId] CHAR (4) NULL,
    CONSTRAINT [PK_TransactionTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TransactionTypes_TransactionAttributeType] FOREIGN KEY ([AttributeId]) REFERENCES [dbo].[accTransactionAttributeTypes] ([Id])
);



