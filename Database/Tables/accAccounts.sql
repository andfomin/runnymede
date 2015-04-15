CREATE TABLE [dbo].[accAccounts] (
    [Id]     INT      IDENTITY (1, 1) NOT NULL,
    [UserId] INT      NOT NULL,
    [Type]   CHAR (6) NOT NULL,
    CONSTRAINT [PK_accAccounts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_accAccounts_Type] CHECK (substring([Type],(1),(2))='AC'),
    CONSTRAINT [FK_accAccounts_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_accAccounts_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id])
);
























GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_UserId_Type]
    ON [dbo].[accAccounts]([UserId] ASC, [Type] ASC);

