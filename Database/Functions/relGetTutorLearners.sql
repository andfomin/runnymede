

CREATE FUNCTION [dbo].[relGetTutorLearners]
(
	@UserId int
)
RETURNS 
	@t TABLE 
	(
		Id int,
		DisplayName nvarchar(100),
		Skype nvarchar(100)
	)
AS
BEGIN

	insert @t
		select LT.LearnerUserId, LT.LearnerDisplayName, U.Skype
		from dbo.relLearnersTutors LT
			left join dbo.appUsers U on LT.TutorUserId = U.Id
		where LT.TutorUserId = @UserId;
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetTutorLearners] TO [websiterole]
    AS [dbo];

