
CREATE FUNCTION [dbo].[relGetTeachers]
(
	@UserId int
)
RETURNS 
	@t TABLE 
	(
		Id int,
		DisplayName nvarchar(100),
		Skype nvarchar(100),
		ReviewRate decimal(18, 2),
		SessionRate decimal(18, 2),
		ExtId uniqueidentifier
	)
AS
BEGIN

	insert @t
		select LT.TeacherUserId, LT.TeacherDisplayName, U.Skype, U.ReviewRate, U.SessionRate, U.ExtId
		from dbo.relLearnersTeachers LT
			left join dbo.appUsers U on LT.TeacherUserId = U.Id
		where LT.LearnerUserId = @UserId;
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetTeachers] TO [websiterole]
    AS [dbo];

