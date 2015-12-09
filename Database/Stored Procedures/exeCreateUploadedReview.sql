

CREATE PROCEDURE [dbo].[exeCreateUploadedReview]
	@ExerciseId int,
	@UserId int -- Reviewer
AS
BEGIN
/*
*/
SET NOCOUNT ON;

	insert dbo.exeReviews (Id, ExerciseId, UserId, RequestTime, StartTime)
	output inserted.Id
		select dbo.exeGetNewReviewId(), @ExerciseId, @UserId, q.[Now], q.[Now]
		from (
			select sysutcdatetime() as [Now]
		) q;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeCreateUploadedReview] TO [websiterole]
    AS [dbo];

