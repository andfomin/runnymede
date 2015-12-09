CREATE TABLE [dbo].[sesSessions] (
    [Id]                 INT            NOT NULL,
    [Start]              SMALLDATETIME  NOT NULL,
    [End]                SMALLDATETIME  NOT NULL,
    [LearnerUserId]      INT            NULL,
    [TeacherUserId]      INT            NULL,
    [ExtId]              BIGINT         NULL,
    [Price]              DECIMAL (9, 2) NOT NULL,
    [BookingTime]        SMALLDATETIME  NULL,
    [ConfirmationTime]   SMALLDATETIME  NULL,
    [CancellationTime]   SMALLDATETIME  NULL,
    [ClosingTime]        SMALLDATETIME  NULL,
    [CancellationUserId] INT            NULL,
    [Rating]             TINYINT        NULL,
    CONSTRAINT [PK_sesSessions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TeacherUserId_LearnerUserId] CHECK ([TeacherUserId]<>[LearnerUserId]),
    CONSTRAINT [FK_sesSessions_appUsers] FOREIGN KEY ([TeacherUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_sesSessions_appUsers1] FOREIGN KEY ([LearnerUserId]) REFERENCES [dbo].[appUsers] ([Id])
);


















































GO
CREATE NONCLUSTERED INDEX [CX_End_Start__LearnerUserId]
    ON [dbo].[sesSessions]([End] ASC, [Start] ASC)
    INCLUDE([LearnerUserId]);

