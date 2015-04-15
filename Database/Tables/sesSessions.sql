CREATE TABLE [dbo].[sesSessions] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [Start]                  SMALLDATETIME  NOT NULL,
    [End]                    SMALLDATETIME  NOT NULL,
    [TeacherUserId]          INT            NULL,
    [LearnerUserId]          INT            NULL,
    [Cost]                   DECIMAL (9, 2) NULL,
    [Price]                  DECIMAL (9, 2) NULL,
    [ProposedTeacherUserId]  INT            NULL,
    [ProposalTime]           SMALLDATETIME  NOT NULL,
    [ProposalAcceptanceTime] SMALLDATETIME  NULL,
    [BookingTime]            SMALLDATETIME  NULL,
    [ClosingTime]            SMALLDATETIME  NULL,
    [Rating]                 TINYINT        NULL,
    CONSTRAINT [PK_sesSessions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_TeacherUserId_LearnerUserId] CHECK ([TeacherUserId]<>[LearnerUserId]),
    CONSTRAINT [FK_sesSessions_appUsers] FOREIGN KEY ([TeacherUserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_sesSessions_appUsers1] FOREIGN KEY ([LearnerUserId]) REFERENCES [dbo].[appUsers] ([Id])
);




































GO
CREATE NONCLUSTERED INDEX [CX_LearnerUserId_End__Start_TeacherUserId]
    ON [dbo].[sesSessions]([LearnerUserId] ASC, [End] ASC)
    INCLUDE([Start], [TeacherUserId]);

