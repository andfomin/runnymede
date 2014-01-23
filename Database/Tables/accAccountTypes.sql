CREATE TABLE [dbo].[accAccountTypes] (
    [Id]      CHAR(4)       NOT NULL,
    [Description]    NVARCHAR (100) NOT NULL,
    [Kind]    NVARCHAR (100) NOT NULL,
    [IsDebit] BIT            NOT NULL,
    CONSTRAINT [PK_AccountTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_AccountTypes_Kind] CHECK ([Kind]='EQUITY' OR [Kind]='LIABILITY' OR [Kind]='ASSET' OR [Kind]='REVENUE' OR [Kind]='EXPENSE')
);




