
CREATE FUNCTION [dbo].[relGetTutors]
(
	@UserId int
)
RETURNS 
	@t TABLE 
	(
		Id int,
		DisplayName nvarchar(100),
		Skype nvarchar(100),
		Rate decimal(18, 2)
	)
AS
BEGIN

	insert @t
		select LT.TutorUserId, LT.TutorDisplayName, U.Skype, U.Rate
		from dbo.relLearnersTutors LT
			left join dbo.appUsers U on LT.TutorUserId = U.Id
		where LT.LearnerUserId = @UserId;
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetTutors] TO [websiterole]
    AS [dbo];

