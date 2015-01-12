CREATE TABLE [dbo].[exeRequests] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [ReviewerUserId] INT            NULL,
    [IsActive]       BIT            NOT NULL,
    [ReviewId]       INT            NOT NULL,
    [Price]          DECIMAL (9, 2) NOT NULL,
    CONSTRAINT [PK_exeRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeRequests_appUsers] FOREIGN KEY ([ReviewerUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeRequests_exeReviews] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[exeReviews] ([Id])
);
















GO



GO



GO



GO
CREATE NONCLUSTERED INDEX [IXFC_ReviewId_IsActive_ReviewerUserId]
    ON [dbo].[exeRequests]([ReviewId] ASC)
    INCLUDE([ReviewerUserId]) WHERE ([IsActive]=(1));


GO
CREATE NONCLUSTERED INDEX [IXFC_ReviewerUserId_IsActive_ReviewId_Price]
    ON [dbo].[exeRequests]([ReviewerUserId] ASC)
    INCLUDE([ReviewId], [Price]) WHERE ([IsActive]=(1));

