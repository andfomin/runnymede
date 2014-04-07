CREATE TABLE [dbo].[relLearnersTeachers] (
    [ClusteredId]        INT            IDENTITY (1, 1) NOT NULL,
    [LearnerUserId]      INT            NOT NULL,
    [TeacherUserId]        INT            NOT NULL,
    [LearnerDisplayName] NVARCHAR (100) NOT NULL,
    [TeacherDisplayName]   NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_relLearnersTeachers] PRIMARY KEY NONCLUSTERED ([LearnerUserId] ASC, [TeacherUserId] ASC)
);












GO
CREATE CLUSTERED INDEX [CI_ClusteredId]
    ON [dbo].[relLearnersTeachers]([ClusteredId] ASC);


GO



GO
GRANT SELECT
    ON OBJECT::[dbo].[relLearnersTeachers] TO [websiterole]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[relLearnersTeachers] TO [websiterole]
    AS [dbo];

