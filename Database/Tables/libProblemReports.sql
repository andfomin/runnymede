CREATE TABLE [dbo].[libProblemReports] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [UserId]          INT             NULL,
    [ResourceId]      INT             NULL,
    [ReportTime]      DATETIME2 (2)   CONSTRAINT [DF_libProblemReports_ReportTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [Report]          NVARCHAR (4000) NULL,
    [ReviewUserId]    INT             NULL,
    [ReviewStartTime] DATETIME2 (2)   NULL,
    [ReviewEndTime]   DATETIME2 (2)   NULL,
    [IsConfirmed]     BIT             NULL,
    CONSTRAINT [PK_libProblemReports] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_libProblemReports_appUsers_Rp] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_libProblemReports_appUsers_Rv] FOREIGN KEY ([ReviewUserId]) REFERENCES [dbo].[appUsers] ([Id])
);







