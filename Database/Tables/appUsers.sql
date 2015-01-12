CREATE TABLE [dbo].[appUsers] (
    [Id]                    INT            NOT NULL,
    [DisplayName]           NVARCHAR (100) NOT NULL,
    [IsTeacher]             BIT            CONSTRAINT [DF_appUsers_IsTeacher] DEFAULT ((0)) NOT NULL,
    [SkypeName]             NVARCHAR (100) NULL,
    [ExtId]                 NCHAR (12)     NULL,
    [CreationTime]          SMALLDATETIME  CONSTRAINT [DF_appUsers_CreationTime] DEFAULT (getutcdate()) NOT NULL,
    [TimezoneOffsetMin]     SMALLINT       NULL,
    [RecordingRate]         DECIMAL (9, 2) NULL,
    [WritingRate]           DECIMAL (9, 2) NULL,
    [SessionRate]           DECIMAL (9, 2) NULL,
    [Announcement]          NVARCHAR (200) NULL,
    [LanguageLevel]         TINYINT        CONSTRAINT [DF_appUsers_LanguageLevel] DEFAULT ((112)) NULL,
    [LanguageLevelMaturity] INT            CONSTRAINT [DF_appUsers_LanguageLevelMaturity] DEFAULT ((1)) NULL,
    CONSTRAINT [PK_appUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_appUsers_aspnetUsers] FOREIGN KEY ([Id]) REFERENCES [dbo].[aspnetUsers] ([Id])
);


















































































GO



GO



GO
GRANT INSERT
    ON OBJECT::[dbo].[appUsers] TO [websiterole]
    AS [dbo];

