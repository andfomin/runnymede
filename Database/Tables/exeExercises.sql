CREATE TABLE [dbo].[exeExercises] (
    [Id]           INT             NOT NULL,
    [UserId]       INT             NOT NULL,
    [CreationTime] DATETIME2 (2)   CONSTRAINT [DF_exeExercises_CreateTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [Type]         CHAR (6)        NOT NULL,
    [Artifact]     NVARCHAR (1000) NULL,
    [Length]       INT             NULL,
    [Title]        NVARCHAR (200)  NULL,
    [CardId]       INT             NULL,
    CONSTRAINT [PK_exeExercises] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_exeExercises_Type] CHECK (substring([Type],(1),(2))='EX'),
    CONSTRAINT [FK_exeExercises_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
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
    INCLUDE([Type]);

