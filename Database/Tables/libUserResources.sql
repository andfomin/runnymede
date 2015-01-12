CREATE TABLE [dbo].[libUserResources] (
    [UserId]              INT            NOT NULL,
    [ResourceId]          INT            NOT NULL,
    [DescriptionId]       INT            NULL,
    [IsPersonal]          BIT            CONSTRAINT [DF_libUserResources_IsCollected] DEFAULT ((0)) NOT NULL,
    [Comment]             NVARCHAR (200) NULL,
    [LanguageLevelRating] TINYINT        NULL,
    [CopycatPriority]     TINYINT        CONSTRAINT [DF_libUserResources_CopycatPriority] DEFAULT ((0)) NOT NULL,
    [ReindexSearch]       BIT            CONSTRAINT [DF_libUserResources_ReindexSearch] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_libUserResources] PRIMARY KEY CLUSTERED ([UserId] ASC, [ResourceId] ASC),
    CONSTRAINT [CK_libUserResources_CopycatPriority] CHECK ([CopycatPriority]<=(5)),
    CONSTRAINT [CK_libUserResources_LanguageLevelRating] CHECK ([LanguageLevelRating]<=(3)),
    CONSTRAINT [FK_libUserResources_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_libUserResources_libResources] FOREIGN KEY ([ResourceId]) REFERENCES [dbo].[libResources] ([Id])
);














GO



GO
GRANT UPDATE
    ON [dbo].[libUserResources] ([ReindexSearch]) TO [websiterole]
    AS [dbo];


GO
CREATE NONCLUSTERED INDEX [FX_ReindexSearch]
    ON [dbo].[libUserResources]([ReindexSearch] ASC) WHERE ([ReindexSearch]=(1));

