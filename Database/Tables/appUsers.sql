CREATE TABLE [dbo].[appUsers] (
    [Id]                INT             NOT NULL,
    [DisplayName]       NVARCHAR (100)  NOT NULL,
    [IsTutor]           BIT             CONSTRAINT [DF_appUsers_IsTutor] DEFAULT ((0)) NOT NULL,
    [CreateTime]        SMALLDATETIME   CONSTRAINT [DF_appUsers_CreateTime] DEFAULT (getutcdate()) NOT NULL,
    [Skype]             NVARCHAR (100)  NULL,
    [TimezoneOffsetMin] SMALLINT        NULL,
    [TimezoneName]      NVARCHAR (50)   NULL,
    [Rate]              DECIMAL (18, 2) NULL,
    [Announcement]      NVARCHAR (1000) NULL,
    CONSTRAINT [PK_appUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
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

