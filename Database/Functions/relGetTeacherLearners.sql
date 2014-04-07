

CREATE FUNCTION [dbo].[relGetTeacherLearners]
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
		from dbo.relLearnersTeachers LT
			left join dbo.appUsers U on LT.TeacherUserId = U.Id
		where LT.TeacherUserId = @UserId;
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetTeacherLearners] TO [websiterole]
    AS [dbo];

