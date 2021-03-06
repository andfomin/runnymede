﻿CREATE TABLE [dbo].[exeExercises] (
    [Id]           INT              NOT NULL,
    [UserId]       INT              NOT NULL,
    [ServiceType]  CHAR (6)         NULL,
    [CardId]       UNIQUEIDENTIFIER NULL,
    [CreationTime] DATETIME2 (2)    CONSTRAINT [DF_exeExercises_CreationTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [ArtifactType] CHAR (6)         NOT NULL,
    [Artifact]     NVARCHAR (1000)  NULL,
    [Length]       DECIMAL (18, 2)  NULL,
    [Title]        NVARCHAR (200)   NULL,
    [Comment]      NVARCHAR (1000)  NULL,
    [Details]      NVARCHAR (4000)  NULL,
    CONSTRAINT [PK_exeExercises] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_exeExercises_ArtifactType] CHECK (substring([ArtifactType],(1),(2))='AR'),
    CONSTRAINT [FK_exeExercises_appTypes] FOREIGN KEY ([ArtifactType]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_exeExercises_appTypes1] FOREIGN KEY ([ServiceType]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_exeExercises_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeExercises_exeCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[exeCards] ([Id])
);
















































GO
GRANT SELECT
    ON OBJECT::[dbo].[exeExercises] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[exeExercises] TO [websiterole]
    AS [dbo];


GO
GRANT UPDATE
    ON [dbo].[exeExercises] ([Title]) TO [websiterole]
    AS [dbo];


GO
CREATE NONCLUSTERED INDEX [CX_UserId_Type]
    ON [dbo].[exeExercises]([UserId] ASC)
    INCLUDE([ArtifactType]);




GO
GRANT UPDATE
    ON [dbo].[exeExercises] ([CardId]) TO [websiterole]
    AS [dbo];

