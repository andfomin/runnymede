CREATE TABLE [dbo].[appMessages] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [Type]                 CHAR (6)       NOT NULL,
    [Attribute]            NVARCHAR (100) NULL,
    [PostTime]             DATETIME2 (2)  CONSTRAINT [DF_appMessages_PostTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [ReceiveTime]          DATETIME2 (2)  NULL,
    [SenderUserId]         INT            NOT NULL,
    [SenderDisplayName]    NVARCHAR (100) NOT NULL,
    [RecipientUserId]      INT            NOT NULL,
    [RecipientDisplayName] NVARCHAR (100) NOT NULL,
    [ExtId]                NCHAR (12)     NULL,
    CONSTRAINT [PK_appMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_appMessages_Type] CHECK (substring([Type],(1),(2))='MS'),
    CONSTRAINT [FK_appMessages_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_appMessages_appUsers_R] FOREIGN KEY ([RecipientUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_appMessages_appUsers_S] FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[appUsers] ([Id])
);










GO
CREATE NONCLUSTERED INDEX [IX_Attribute_Type]
    ON [dbo].[appMessages]([Attribute] ASC, [Type] ASC);

