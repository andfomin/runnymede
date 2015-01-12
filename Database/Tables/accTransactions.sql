CREATE TABLE [dbo].[accTransactions] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Type]         CHAR (6)       NOT NULL,
    [ObservedTime] DATETIME2 (2)  CONSTRAINT [DF__accTransa__Obser__2F10007B] DEFAULT (sysutcdatetime()) NOT NULL,
    [Attribute]    NVARCHAR (100) NULL,
    [Details]      XML            NULL,
    CONSTRAINT [PK_accTransactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_accTransactions_Type] CHECK (substring([Type],(1),(2))='TR'),
    CONSTRAINT [FK_accTransactions_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id])
);





















