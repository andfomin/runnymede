CREATE TABLE [dbo].[exeRemarks] (
    [ReviewId]     INT             NOT NULL,
    [CreationTime] INT             NOT NULL,
    [Start]        INT             NULL,
    [Finish]       INT             NULL,
    [Text]         NVARCHAR (1000) NULL,
    [Keywords]     NVARCHAR (1000) NULL,
    CONSTRAINT [PK_exeRemarks] PRIMARY KEY CLUSTERED ([ReviewId] ASC, [CreationTime] ASC),
    CONSTRAINT [FK_exeRemarks_exeReviews] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[exeReviews] ([Id])
);








GO
GRANT SELECT
    ON OBJECT::[dbo].[exeRemarks] TO [websiterole]
    AS [dbo];

