CREATE TABLE [dbo].[relLearnersTutors] (
    [ClusteredId]        INT            IDENTITY (1, 1) NOT NULL,
    [LearnerUserId]      INT            NOT NULL,
    [TutorUserId]        INT            NOT NULL,
    [LearnerDisplayName] NVARCHAR (100) NOT NULL,
    [TutorDisplayName]   NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_relLearnersTutors] PRIMARY KEY NONCLUSTERED ([LearnerUserId] ASC, [TutorUserId] ASC)
);












GO
CREATE CLUSTERED INDEX [CI_ClusteredId]
    ON [dbo].[relLearnersTutors]([ClusteredId] ASC);


GO



GO
GRANT SELECT
    ON OBJECT::[dbo].[relLearnersTutors] TO [websiterole]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[relLearnersTutors] TO [websiterole]
    AS [dbo];

