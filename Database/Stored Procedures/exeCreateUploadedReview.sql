

CREATE PROCEDURE [dbo].[exeCreateUploadedReview]
	@ExerciseId int,
	@UserId int -- Reviewer
AS
BEGIN
/*
*/
SET NOCOUNT ON;

	insert dbo.exeReviews (Id, ExerciseId, UserId, Price, RequestTime, StartTime)
	output inserted.Id
		select dbo.exeGetNewReviewId(), @ExerciseId, @UserId, 0, q.NowTime, q.NowTime
		from (
			select sysutcdatetime() as NowTime
		) q;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCreateUploadedReview] TO [websiterole]
    AS [dbo];

