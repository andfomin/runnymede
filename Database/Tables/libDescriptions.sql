CREATE TABLE [dbo].[libDescriptions] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [TitleId]        BIGINT         NULL,
    [CategoryIds]    NVARCHAR (50)  NULL,
    [Tags]           NVARCHAR (100) NULL,
    [SourceId]       NCHAR (4)      NULL,
    [HasExplanation] BIT            CONSTRAINT [DF_libDescriptions_HasExplanation] DEFAULT ((0)) NOT NULL,
    [HasExample]     BIT            CONSTRAINT [DF_libDescriptions_HasExample] DEFAULT ((0)) NOT NULL,
    [HasExercise]    BIT            CONSTRAINT [DF_libDescriptions_HasExercise] DEFAULT ((0)) NOT NULL,
    [HasText]        BIT            CONSTRAINT [DF_libDescriptions_HasText] DEFAULT ((0)) NOT NULL,
    [HasPicture]     BIT            CONSTRAINT [DF_libDescriptions_HasPicture] DEFAULT ((0)) NOT NULL,
    [HasAudio]       BIT            CONSTRAINT [DF_libDescriptions_HasAudio] DEFAULT ((0)) NOT NULL,
    [HasVideo]       BIT            CONSTRAINT [DF_libDescriptions_HasVideo] DEFAULT ((0)) NOT NULL,
    [Checksum]       AS             (CONVERT([bigint],(4294967296.))*binary_checksum([TitleId],[CategoryIds],[Tags],[SourceId],[HasExplanation],[HasExample],[HasExercise],[HasText],[HasPicture],[HasAudio],[HasVideo])+binary_checksum(reverse([CategoryIds]),reverse([Tags]),reverse([SourceId]))),
    [CreationTime]   SMALLDATETIME  CONSTRAINT [DF_libDescriptions_CreationTime] DEFAULT (sysutcdatetime()) NULL,
    CONSTRAINT [PK_libDescriptions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_libDescriptions_libSources] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[libSources] ([Id]),
    CONSTRAINT [FK_libDescriptions_libTitles] FOREIGN KEY ([TitleId]) REFERENCES [dbo].[libTitles] ([Id])
);


























GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Checksum]
    ON [dbo].[libDescriptions]([Checksum] ASC);

