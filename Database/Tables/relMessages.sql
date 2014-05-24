CREATE TABLE [dbo].[relMessages] (
    [Id]                   INT             IDENTITY (1, 1) NOT NULL,
    [Type]                 NCHAR (4)       NOT NULL,
    [PostTime]             DATETIME2 (2)   CONSTRAINT [DF_relMessages_PostTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [ReceiveTime]          DATETIME2 (2)   NULL,
    [SenderUserId]         INT             NOT NULL,
    [RecipientUserId]      INT             NOT NULL,
    [SenderDisplayName]    NVARCHAR (100)  NOT NULL,
    [RecepientDisplayName] NVARCHAR (100)  NOT NULL,
    [Attribute]            NVARCHAR (100)  NULL,
    [Text]                 NVARCHAR (1000) NULL,
    CONSTRAINT [PK_relMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_relMessages_appUsers] FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_relMessages_appUsers1] FOREIGN KEY ([RecipientUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_relMessages_relMessageTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[relMessageTypes] ([Id])
);








GO
CREATE NONCLUSTERED INDEX [IX_Attribute_Type]
    ON [dbo].[relMessages]([Attribute] ASC, [Type] ASC);

