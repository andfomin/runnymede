CREATE TYPE [dbo].[exeRemarksType] AS TABLE (
    [ReviewId]     INT             NOT NULL,
    [CreationTime] INT             NOT NULL,
    [Start]        INT             NOT NULL,
    [Finish]       INT             NOT NULL,
    [Text]         NVARCHAR (1000) NULL,
    [Keywords]     NVARCHAR (1000) NULL,
    PRIMARY KEY CLUSTERED ([ReviewId] ASC, [CreationTime] ASC));




GO
GRANT EXECUTE
    ON TYPE::[dbo].[exeRemarksType] TO [websiterole]
    AS [dbo];

