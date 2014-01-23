
CREATE FUNCTION [dbo].[relGetRelatedTutors]
(
	@UserId int
)
RETURNS 
	@t TABLE 
	(
		Id int,
		DisplayName nvarchar(100),
		RateARec decimal(18, 2)
	)
AS
BEGIN

	insert @t
		select LT.TutorUserId, LT.TutorDisplayName, coalesce(LT.T2LRateARec, U.RateARec)
		from dbo.relLearnersTutors LT
			left join dbo.appUsers U on LT.TutorUserId = U.Id
		where LT.LearnerUserId = @UserId
			and LT.L2TDate is not null;
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetRelatedTutors] TO [websiterole]
    AS [dbo];

