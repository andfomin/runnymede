CREATE TABLE [dbo].[sesSessions] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [HostUserId]         INT            NOT NULL,
    [GuestUserId]        INT            NOT NULL,
    [Start]              SMALLDATETIME  NOT NULL,
    [End]                SMALLDATETIME  NOT NULL,
    [Price]              DECIMAL (9, 2) NOT NULL,
    [RequestTime]        DATETIME2 (2)  CONSTRAINT [DF_sesSessions_RequestTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [ConfirmationTime]   DATETIME2 (2)  NULL,
    [CancellationTime]   DATETIME2 (2)  NULL,
    [CancellationUserId] INT            NULL,
    [DisputeTimeByHost]  DATETIME2 (2)  NULL,
    [DisputeTimeByGuest] DATETIME2 (2)  NULL,
    [FinishTime]         DATETIME2 (2)  NULL,
    [GuestHasVacantTime] BIT            NULL,
    CONSTRAINT [PK_sesSessions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_HostUserId_GuestUserId] CHECK ([HostUserId]<>[GuestUserId])
);









