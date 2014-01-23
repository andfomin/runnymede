CREATE TABLE [dbo].[accTransactions] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [TransactionTypeId] CHAR (4)      NOT NULL,
    [ObservedTime]      DATETIME2 (7) CONSTRAINT [DF__accTransa__Obser__2F10007B] DEFAULT (sysutcdatetime()) NOT NULL,
    [Attribute]         INT           NULL,
    [Details]           XML           NULL,
    CONSTRAINT [PK_accTransactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_accTransactions_accTransactionTypeId] FOREIGN KEY ([TransactionTypeId]) REFERENCES [dbo].[accTransactionTypes] ([Id])
);









