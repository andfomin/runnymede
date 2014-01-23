

CREATE PROCEDURE [dbo].[exeStartReview]
	@ReviewId int,
	@UserId int
AS
BEGIN
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ExerciseId int, @ReviewerName nvarchar(100), @HasAnother bit, @HasReviewed bit;
	
	select @ExerciseId = ExerciseId from dbo.exeReviews where Id = @ReviewId and StartTime is null;

	if @ExerciseId is null 
		raiserror('%s,%d:: The review has already been started.', 16, 1, @ProcName, @ReviewId);

	select @HasAnother = max(case when FinishTime is null then 1 else 0 end), 
		@HasReviewed = max(case when ExerciseId = @ExerciseId then 1 else 0 end)
	from dbo.exeReviews 
	where UserId = @UserId;

	if @HasAnother = 1 
		raiserror('%s,%d:: The user has another ongoing review.', 16, 1, @ProcName, @UserId);

	if @HasReviewed = 1
		raiserror('%s,%d,%d:: The user has already reviewed the exercise.', 16, 1, @ProcName, @UserId, @ExerciseId);

	select @ReviewerName = DisplayName from dbo.appUsers where Id = @UserId and IsTutor = 1

	if @ReviewerName is null 
		raiserror('%s,%d:: The user is not a tutor.', 16, 1, @ProcName, @UserId);

	if not exists (select * from dbo.exeRequests where ReviewId = @ReviewId and coalesce(ReviewerUserId, @UserId) = @UserId) 
		raiserror('%s,%d,%d:: The user is not allowed to start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
			set UserId = @UserId, ReviewerName = @ReviewerName, StartTime = sysutcdatetime()
			where Id = @ReviewId and StartTime is null;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

		delete dbo.exeRequests
			output deleted.Id, deleted.ReviewId, deleted.ReviewerUserId
			into dbo.exeRequestsArchive (RequestId, ReviewId, ReviewerUserId)
		where ReviewId = @ReviewId

	if @ExternalTran = 0
		commit;
end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeStartReview] TO [websiterole]
    AS [dbo];

