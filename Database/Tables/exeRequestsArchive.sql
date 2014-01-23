CREATE TABLE [dbo].[exeRequestsArchive] (
    [ClusterdId]     INT IDENTITY (1, 1) NOT NULL,
    [RequestId]      INT NOT NULL,
    [ReviewId]       INT NOT NULL,
    [ReviewerUserId] INT NULL,
    CONSTRAINT [PK_exeRequestsArchive] PRIMARY KEY NONCLUSTERED ([RequestId] ASC)
);




GO
CREATE CLUSTERED INDEX [CX_exeRequestsArchive]
    ON [dbo].[exeRequestsArchive]([ClusterdId] ASC);

