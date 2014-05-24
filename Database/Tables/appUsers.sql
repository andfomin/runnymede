CREATE TABLE [dbo].[appUsers] (
    [Id]                INT              NOT NULL,
    [DisplayName]       NVARCHAR (100)   NOT NULL,
    [IsTeacher]         BIT              CONSTRAINT [DF_appUsers_IsTeacher] DEFAULT ((0)) NOT NULL,
    [CreateTime]        SMALLDATETIME    CONSTRAINT [DF_appUsers_CreateTime] DEFAULT (getutcdate()) NOT NULL,
    [Skype]             NVARCHAR (100)   NULL,
    [TimezoneOffsetMin] SMALLINT         NULL,
    [TimezoneName]      NVARCHAR (50)    NULL,
    [ReviewRate]        DECIMAL (18, 2)  NULL,
    [SessionRate]       DECIMAL (18, 2)  NULL,
    [Announcement]      NVARCHAR (1000)  NULL,
    [ExtId]             UNIQUEIDENTIFIER NULL,
    [ExtIdUpperCase]    AS               (upper(CONVERT([nchar](36),[ExtId]))),
    [CommentWeight]     TINYINT          CONSTRAINT [DF_appUsers_CommentWeight] DEFAULT ((1)) NOT NULL,
    [ReferenceLevel]    NCHAR (2)        NULL,
    CONSTRAINT [PK_appUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_appUsers_appReferenceLevels] FOREIGN KEY ([ReferenceLevel]) REFERENCES [dbo].[appReferenceLevels] ([Id]),
    CONSTRAINT [FK_appUsers_aspnetUsers] FOREIGN KEY ([Id]) REFERENCES [dbo].[aspnetUsers] ([Id])
);














































GO
GRANT UPDATE
    ON OBJECT::[dbo].[appUsers] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[appUsers] TO [websiterole]
    AS [dbo];

