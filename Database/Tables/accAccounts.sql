CREATE TABLE [dbo].[accAccounts] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [UserId]        INT      NOT NULL,
    [AccountTypeId] CHAR (4) NOT NULL,
    CONSTRAINT [PK_accAccounts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_accAccounts_accAccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [dbo].[accAccountTypes] ([Id]),
    CONSTRAINT [FK_accAccounts_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id])
);















