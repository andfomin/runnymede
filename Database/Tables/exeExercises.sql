CREATE TABLE [dbo].[exeExercises] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     INT            NOT NULL,
    [CreateTime] DATETIME2 (2)  CONSTRAINT [DF_exeExercises_CreateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [TypeId]     NCHAR (4)      NOT NULL,
    [ArtefactId] NVARCHAR (100) NULL,
    [TopicId]    NCHAR (8)      NULL,
    [Length]     INT            NULL,
    [Title]      NVARCHAR (100) NULL,
    CONSTRAINT [PK_exeExercises] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeExercises_appUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[appUsers] ([Id]),
    CONSTRAINT [FK_exeExercises_exeExerciseTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[exeExerciseTypes] ([Id])
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

