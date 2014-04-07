



CREATE PROCEDURE [dbo].[accFinishReview]
	@ReviewId int
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

	declare @ExerciseId int, @AuthorUserId int, @ReviewerUserId int,
		@Reward decimal(18,2), @ServiceFeeRate float, @Fee decimal(18,2), @Attribute nvarchar(100),
		@AuthorAccountId int,  @ReviewerAccountId int, @RevenueAccountId int, 
		@RewardTransactionId int, @FeeTransactionId int, @InitialReviewerBalance decimal(18,2);

	select @ExerciseId = ExerciseId, @Reward = Reward, @ReviewerUserId = UserId
	from dbo.exeReviews 
	where Id = @ReviewId;

	select @AuthorUserId = UserId 
	from dbo.exeExercises 
	where Id = @ExerciseId;

	set @AuthorAccountId = dbo.accGetEcrowAccount(@AuthorUserId);

	set @ReviewerAccountId = dbo.accGetPersonalAccount(@ReviewerUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@AuthorAccountId is null) or (@ReviewerAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d,%d:: One or more accounts not found.', 16, 1, @ProcName, @AuthorUserId, @ReviewerUserId);

	set @ServiceFeeRate = dbo.appGetConstantAsFloat('Exercises.Reviews.ServiceFeeRate');
	set @Fee = cast((@Reward * @ServiceFeeRate) as decimal(18,2));

	set @Attribute = cast(@ReviewId as nvarchar(100));

	declare @Now datetime2(0) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		-- Transfer reward.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('EXRF', @Now, @Attribute);

		select @RewardTransactionId = scope_identity() where @@rowcount != 0;

		-- Deduct service fee.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('EXFD', @Now, @Attribute);

		select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the author account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @RewardTransactionId, @AuthorAccountId, @Reward, null, Balance - @Reward
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @AuthorAccountId);

		-- We are going to post two entries. Reuse the balance.
		select @InitialReviewerBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @ReviewerAccountId);

		-- Temporary credit the reviewer account with the full reward amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@RewardTransactionId, @ReviewerAccountId, null, @Reward, @InitialReviewerBalance + @Reward)

		-- Deduct service fee from the reviewer account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @ReviewerAccountId, @Fee, null, @InitialReviewerBalance + @Reward - @Fee)

		-- Credit the service.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @FeeTransactionId, @RevenueAccountId, null, @Fee, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

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