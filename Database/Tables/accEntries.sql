CREATE TABLE [dbo].[accEntries] (
    [Id]            BIGINT          IDENTITY (1, 1) NOT NULL,
    [TransactionId] INT             NOT NULL,
    [AccountId]     INT             NOT NULL,
    [Debit]         DECIMAL (9, 2)  NULL,
    [Credit]        DECIMAL (9, 2)  NULL,
    [Balance]       DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_accEntries] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Entries_Debit_Credit] CHECK ([Debit] IS NOT NULL AND [Credit] IS NULL OR [Debit] IS NULL AND [Credit] IS NOT NULL),
    CONSTRAINT [FK_accEntries_accAccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[accAccounts] ([Id]),
    CONSTRAINT [FK_accEntries_accTransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [dbo].[accTransactions] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AccountId_Id]
    ON [dbo].[accEntries]([AccountId] ASC, [Id] ASC);

