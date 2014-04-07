

CREATE PROCEDURE [dbo].[exeCreateUploadedReview]
	@ExerciseId int,
	@UserId int -- Reviewer
AS
BEGIN
/*
*/
SET NOCOUNT ON;

	insert dbo.exeReviews (ExerciseId, UserId, Reward, RequestTime, StartTime, AuthorName, ReviewerName)
	output inserted.Id
		select @ExerciseId, @UserId, 0, q1.NowTime, q1.NowTime, q1.DisplayName, q2.DisplayName
		from (
			select U.DisplayName, sysutcdatetime() as NowTime
			from dbo.appUsers U 
				inner join dbo.exeExercises E on U.Id = E.UserId
			where E.Id = @ExerciseId
		) q1,
		(
			select DisplayName from dbo.appUsers where Id = @UserId
		) q2;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCreateUploadedReview] TO [websiterole]
    AS [dbo];

