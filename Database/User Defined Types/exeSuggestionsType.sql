CREATE TYPE [dbo].[exeSuggestionsType] AS TABLE (
    [ReviewId]     INT             NOT NULL,
    [CreationTime] INT             NOT NULL,
    [Text]         NVARCHAR (1000) NULL,
    PRIMARY KEY CLUSTERED ([ReviewId] ASC, [CreationTime] ASC));


GO
GRANT EXECUTE
    ON TYPE::[dbo].[exeSuggestionsType] TO [websiterole]
    AS [dbo];

