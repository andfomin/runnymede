

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

	declare @ExerciseId int, @AuthorUserId int, @ReviewerName nvarchar(100), @Attribute nvarchar(100), @Now datetime2(2);		
	
	select @ExerciseId = R.ExerciseId, @AuthorUserId = E.UserId
	from dbo.exeReviews R
		inner join dbo.exeExercises E on R.ExerciseId = E.Id
	where R.Id = @ReviewId 
		and R.StartTime is null;

	if @ExerciseId is null 
		raiserror('%s,%d:: The review has already been started.', 16, 1, @ProcName, @ReviewId);

	select @ReviewerName = DisplayName 
	from dbo.appUsers 
	where Id = @UserId
		and IsTeacher = 1;

	if (@ReviewerName is null)
		raiserror('%s,%d,%d:: The user cannot start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

	-- There is index IX_UserId_FinishTime
	if exists (
		select *
		from dbo.exeReviews 
		where UserId = @UserId 
			and FinishTime is null
	)
		raiserror('%s,%d:: The user has another ongoing review.', 16, 1, @ProcName, @UserId);

	if exists (
		select * 
		from dbo.exeReviews
		where ExerciseId = @ExerciseId
			and UserId = @UserId
	)
		raiserror('%s,%d:: The user has already reviewed this exercise.', 16, 1, @ProcName, @UserId);

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
		set UserId = @UserId, StartTime = @Now
		where Id = @ReviewId 
			and StartTime is null;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

	if @ExternalTran = 0
		commit;

	-- ExerciseId is the PartitionKey in the storage table. We add access records for both the author and the reviewer. And we will notify the author in real-time.
	select @ExerciseId as ExerciseId, @Now as StartTime, @UserId as ReviewerUserId, @ReviewerName as ReviewerName, @AuthorUserId as AuthorUserId;

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

