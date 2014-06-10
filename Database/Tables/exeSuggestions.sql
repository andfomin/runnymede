CREATE TABLE [dbo].[exeSuggestions] (
    [ReviewId]     INT             NOT NULL,
    [CreationTime] INT             NOT NULL,
    [Text]         NVARCHAR (1000) NULL,
    CONSTRAINT [PK_exeSuggestions] PRIMARY KEY CLUSTERED ([ReviewId] ASC, [CreationTime] ASC),
    CONSTRAINT [FK_exeSuggestions_exeReviews] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[exeReviews] ([Id])
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[exeSuggestions] TO [websiterole]
    AS [dbo];

