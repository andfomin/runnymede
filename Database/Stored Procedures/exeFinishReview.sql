﻿


CREATE PROCEDURE [dbo].[exeFinishReview]
	@ReviewId int,
	@UserId int
AS
BEGIN
/*
20121117 AF.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ExerciseId int, @Reward decimal(18,2), @ServiceFeeRate float, @Fee decimal(18,2), @AuthorUserId int, @Attribute nvarchar(100),
		@AuthorAccountId int,  @ReviewerAccountId int, @RevenueAccountId int, 
		@RewardTransactionId int, @FeeTransactionId int, @InitialReviewerBalance decimal(18,2);

	select @ExerciseId = ExerciseId, @Reward = Reward 
	from dbo.exeReviews 
	where Id = @ReviewId
		and UserId = @UserId 
		and FinishTime is null;

	if @ExerciseId is null
		raiserror('%s,%d,%d:: The user cannot finish the review.', 16, 1, @ProcName, @UserId, @ReviewId);

	select @AuthorUserId = UserId from dbo.exeExercises	where Id = @ExerciseId;

	declare @Now datetime2(0) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
		set FinishTime = @Now
		where Id = @ReviewId 
			and UserId = @UserId 
			and FinishTime is null;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to finish the review.', 16, 1, @ProcName, @UserId, @ReviewId);

		exec dbo.accFinishReview @ReviewId, @AuthorUserId, @UserId;

	if @ExternalTran = 0
		commit;

	select @Now;

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
    ON OBJECT::[dbo].[exeFinishReview] TO [websiterole]
    AS [dbo];

