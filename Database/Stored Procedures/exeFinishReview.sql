


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

	declare @ExerciseId int, @Reward decimal(18,2), @ServiceFeeRate float, @Fee decimal(18,2), @AuthorUserId int,
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

	set @ServiceFeeRate = cast(dbo.appGetConstant('Exercises.Reviews.ServiceFeeRate') as float);
	set @Fee = cast((@Reward * @ServiceFeeRate) as decimal(18,2));

	set @AuthorAccountId = dbo.accGetEcrowAccount(@AuthorUserId);

	set @ReviewerAccountId = dbo.accGetPersonalAccount(@UserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@AuthorAccountId is null) or (@ReviewerAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d,%d:: One or more accounts not found for the review or the user.', 16, 1, @ProcName, @ReviewId, @UserId);

	declare @Now datetime2(0) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
		set FinishTime = @Now
		where Id = @ReviewId 
			and UserId = @UserId 
			and FinishTime is null;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to finish the review.', 16, 1, @ProcName, @ReviewId, @UserId);

		-- Transfer reward.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute, Details)
		values ('EXRF', @Now, @ReviewId, null);

		select @RewardTransactionId = scope_identity() where @@rowcount != 0;

		-- Debit the author.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @RewardTransactionId, @AuthorAccountId, @Reward, null, Balance - @Reward
			from dbo.accEntries
			where AccountId = @AuthorAccountId
			order by Id desc;

		-- Credit the reviewer account with the full reward amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @RewardTransactionId, @ReviewerAccountId, null, @Reward, Balance + @Reward
			from dbo.accEntries
			where AccountId = @ReviewerAccountId
			order by Id desc;

/* We pospone deducting service fee until the tutor withdraws money/end of month. That way the transaction list on the tutor's balance page looks more pleasant. */

		------ We are going to post two enties. Reuse the balance.
		----select top(1) @InitialReviewerBalance = Balance
		----from dbo.accEntries
		----where AccountId = @ReviewerAccountId
		----order by Id desc;

		------ Temporary credit the reviewer account with the full reward amount.
		----insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		----	values (@RewardTransactionId, @ReviewerAccountId, null, @Reward, @InitialReviewerBalance + @Reward)

		------ Deduct service fee.
		----insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute, Details)
		----values ('EXFD', @Now, @ReviewId, null);

		----select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		------ Deduct service fee from the reviewer account.
		----insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		----	values (@FeeTransactionId, @ReviewerAccountId, @Fee, null, @InitialReviewerBalance + @Reward - @Fee)

		------ Credit the service.
		----insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		----	select top(1) @FeeTransactionId, @RevenueAccountId, null, @Fee, E.Balance + @Fee
		----	from dbo.accEntries E
		----	where E.AccountId = @RevenueAccountId
		----	order by E.Id desc;

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

