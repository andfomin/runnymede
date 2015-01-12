

CREATE PROCEDURE [dbo].[exeCreateUploadedReview]
	@ExerciseId int,
	@UserId int -- Reviewer
AS
BEGIN
/*
*/
SET NOCOUNT ON;

	insert dbo.exeReviews (Id, ExerciseId, UserId, Price, ExerciseType, ExerciseLength, RequestTime, StartTime, AuthorUserId, AuthorName, ReviewerName)
	output inserted.Id
		select dbo.exeGetNewReviewId(), @ExerciseId, @UserId, 0, q1.[Type], q1.[Length], q1.NowTime, q1.NowTime, q1.Id, q1.DisplayName, q2.DisplayName
		from (
			select E.[Type], E.[Length], U.Id, U.DisplayName, sysutcdatetime() as NowTime
			from dbo.exeExercises E 
				inner join dbo.appUsers U on E.UserId = U.Id
			where E.Id = @ExerciseId
		) q1,
		(
			select DisplayName 
			from dbo.appUsers 
			where Id = @UserId
		) q2;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCreateUploadedReview] TO [websiterole]
    AS [dbo];

