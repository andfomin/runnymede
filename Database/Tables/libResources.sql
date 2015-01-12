CREATE TABLE [dbo].[libResources] (
    [Id]                    INT             NOT NULL,
    [Format]                CHAR (6)        NULL,
    [NaturalKey]            NVARCHAR (2000) NOT NULL,
    [Segment]               NVARCHAR (1000) NULL,
    [IsCommon]              BIT             NULL,
    [IsForCopycat]          BIT             CONSTRAINT [DF_libResources_ForCopycat] DEFAULT ((0)) NOT NULL,
    [DescriptionId]         INT             NULL,
    [LanguageLevel]         TINYINT         NULL,
    [LanguageLevelMaturity] INT             NULL,
    [Rating]                TINYINT         CONSTRAINT [DF_libResources_Rating] DEFAULT ((0)) NULL,
    [IndexedLanguageLevel]  TINYINT         NULL,
    [IndexedRating]         TINYINT         NULL,
    [ReindexSearch]         BIT             CONSTRAINT [DF_libResources_ReindexSearch] DEFAULT ((0)) NOT NULL,
    [UserId]                INT             NOT NULL,
    [Checksum]              AS              (CONVERT([bigint],(4294967296.))*binary_checksum([Format],[NaturalKey],[Segment])+binary_checksum(reverse([Segment]),reverse([NaturalKey]),reverse([Format]))),
    [NaturalKeyChecksum]    AS              (CONVERT([bigint],(4294967296.))*binary_checksum([NaturalKey])+binary_checksum(reverse([NaturalKey]))),
    CONSTRAINT [PK_libResources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_libResources_Format] CHECK (substring([Format],(1),(2))='FR'),
    CONSTRAINT [FK_libResources_appTypes] FOREIGN KEY ([Format]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_libResources_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_libResources_libDescriptions] FOREIGN KEY ([DescriptionId]) REFERENCES [dbo].[libDescriptions] ([Id])
);






































GO



GO



GO



GO



GO



GO



GO



GO



GO
GRANT UPDATE
    ON [dbo].[libResources] ([ReindexSearch]) TO [websiterole]
    AS [dbo];


GO



GO
GRANT SELECT
    ON [dbo].[libResources] ([IsCommon]) TO [websiterole]
    AS [dbo];


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Checksum]
    ON [dbo].[libResources]([Checksum] ASC);


GO
CREATE NONCLUSTERED INDEX [FX_NaturalKeyChecksum]
    ON [dbo].[libResources]([NaturalKeyChecksum] ASC) WHERE ([IsForCopycat]=(1));

