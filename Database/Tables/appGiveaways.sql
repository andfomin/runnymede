CREATE TABLE [dbo].[appGiveaways] (
    [UserId]      INT      NOT NULL,
    [ServiceType] CHAR (6) NOT NULL,
    [Counter]     INT      NULL,
    CONSTRAINT [PK_appGiveaways] PRIMARY KEY CLUSTERED ([UserId] ASC, [ServiceType] ASC),
    CONSTRAINT [FK_appGiveaways_appTypes] FOREIGN KEY ([ServiceType]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_appGiveaways_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id])
);

