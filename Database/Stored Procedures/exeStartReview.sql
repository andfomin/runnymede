

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

	declare @ExerciseId int, @AuthorUserId int, @ReviewerName nvarchar(100), @Attribute nvarchar(100),
		@InitialPrice decimal(9,2), @ActualPrice decimal(9,2), @RefundAmount decimal(9,2);
	
	select @ExerciseId = R.ExerciseId, @InitialPrice = R.Price, @AuthorUserId = E.UserId
	from dbo.exeReviews R
		inner join dbo.exeExercises E on R.ExerciseId = E.Id
	where R.Id = @ReviewId 
		and R.StartTime is null;

	if @ExerciseId is null 
		raiserror('%s,%d:: The review has already been started.', 16, 1, @ProcName, @ReviewId);

	-- There is a filtered index
	if exists (
		select *
		from dbo.exeReviews 
		where UserId = @UserId 
			and FinishTime is null
	)
		raiserror('%s,%d:: The user has another ongoing review.', 16, 1, @ProcName, @UserId);

	select @ReviewerName = DisplayName 
	from dbo.appUsers 
	where Id = @UserId;

	-- We store intially the maximal price in dbo.exeReviews. We store the personal prices in dbo.exeRequests.
	-- We will adjust it and refund the user on the review start based on the actual price of the reviewer.
	-- If here are both a personal and a common requests simultaneously, the personal price wins.
	select @ActualPrice = 
		iif(
			count(*) = 1, 
			max(Price), 
			max(iif(ReviewerUserId = @UserId, Price, 0))
		)
	from dbo.exeRequests
	where ReviewId = @ReviewId
		and IsActive = 1
		and coalesce(ReviewerUserId, @UserId) = @UserId;

	if @ActualPrice is null
		raiserror('%s,%d,%d:: The user cannot start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
		set UserId = @UserId, Price = @ActualPrice, ReviewerName = @ReviewerName, StartTime = sysutcdatetime()
		where Id = @ReviewId 
			and StartTime is null;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to start the review.', 16, 1, @ProcName, @UserId, @ReviewId);

		update dbo.exeRequests
		set IsActive = 0
		where ReviewId = @ReviewId;

		if (@InitialPrice > @ActualPrice) begin

			-- Should be negative. The sign determines the transfer direction.
			set @RefundAmount = @InitialPrice - @ActualPrice;

			set @Attribute = cast(@ReviewId as nvarchar(100));

			exec dbo.accChangeEscrow @UserId = @AuthorUserId, @Amount = @RefundAmount, @TransactionType = 'TRRVST', @Attribute = @Attribute;
		end

		exec dbo.friUpdateLastContact @UserId = @UserId, @FriendUserId = @AuthorUserId, @ContactType = 'CN__RS';

	if @ExternalTran = 0
		commit;

	-- ExerciseId is as PartitionKey in the storage table.
	select @ExerciseId;

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

