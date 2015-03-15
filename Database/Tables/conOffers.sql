CREATE TABLE [dbo].[conOffers] (
    [CreateTime] DATETIME2 (2) NOT NULL,
    [UserId]     INT           NOT NULL,
    [ToUserId]   INT           NOT NULL,
    CONSTRAINT [PK_conOffers] PRIMARY KEY CLUSTERED ([CreateTime] ASC, [UserId] ASC),
    CONSTRAINT [FK_conOffers_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_conOffers_appUsers1] FOREIGN KEY ([ToUserId]) REFERENCES [dbo].[appUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [CX_CreateTime_ToUserId_UserId]
    ON [dbo].[conOffers]([CreateTime] ASC, [ToUserId] ASC)
    INCLUDE([UserId]);


GO
GRANT SELECT
    ON OBJECT::[dbo].[conOffers] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[conOffers] TO [websiterole]
    AS [dbo];

