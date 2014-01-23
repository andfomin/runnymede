CREATE TABLE [dbo].[exeRequests] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [ReviewId]       INT             NOT NULL,
    [ReviewerUserId] INT             NULL,
    [Reward]         DECIMAL (18, 2) NULL,
    [AuthorName]     NVARCHAR (100)  NOT NULL,
    [TypeId]         NCHAR (4)       NULL,
    [Length]         INT             NULL,
    CONSTRAINT [PK_exeRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeRequests_appUsers] FOREIGN KEY ([ReviewerUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeRequests_exeReviews] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[exeReviews] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ReviewerUserId_RewievId]
    ON [dbo].[exeRequests]([ReviewerUserId] ASC, [ReviewId] ASC);


GO
GRANT SELECT
    ON OBJECT::[dbo].[exeRequests] TO [websiterole]
    AS [dbo];

