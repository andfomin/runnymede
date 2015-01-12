
CREATE PROCEDURE [dbo].[accFinishReview]
	@ReviewId int
AS
BEGIN
/*
20121117 AF.
20140912 AF.
This procedure assumes that the Price of the review is non-zero.

It is called internally by dbo.exeFinishReview, so do not set access permitions on it.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ExerciseId int, @AuthorUserId int, @ReviewerUserId int,
		@Price decimal(9,2), @PriceRate float, @Fee decimal(9,2), @Attribute nvarchar(100),
		@AuthorAccountId int,  @ReviewerAccountId int, @RevenueAccountId int, 
		@PriceTransactionId int, @FeeTransactionId int, @InitialReviewerBalance decimal(18,2),
		@Now datetime2(2);

	select @ExerciseId = ExerciseId, @ReviewerUserId = UserId, @Price = Price,
		@Fee = convert(decimal(9,2), Price * dbo.appGetFeeRate(ExerciseType, RequestTime, Price / ExerciseLength))
	from dbo.exeReviews 
	where Id = @ReviewId
		and StartTime is not null
		and FinishTime is null;

	-- Price is not nullable.
	if coalesce(@Price, -1) < 0
		raiserror('%s,%d:: The review cannot be finished.', 16, 1, @ProcName, @ReviewId);

	select @AuthorUserId = UserId
	from dbo.exeExercises 
	where Id = @ExerciseId;

	set @AuthorAccountId = dbo.accGetEcrowAccount(@AuthorUserId);

	set @ReviewerAccountId = dbo.accGetPersonalAccount(@ReviewerUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@AuthorAccountId is null) or (@ReviewerAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d,%d:: Account not found.', 16, 1, @ProcName, @AuthorUserId, @ReviewerUserId);

	set @Attribute = convert(nvarchar(100), @ReviewId);

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews
			set FinishTime = @Now
		where Id = @ReviewId 
			and StartTime is not null
			and FinishTime is null;

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to finish the review.', 16, 1, @ProcName, @ReviewId);

		-- Transfer reward.
		insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRRVFN', @Now, @Attribute);

		select @PriceTransactionId = scope_identity() where @@rowcount != 0;

		-- Deduct service fee.
		insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRRVFD', @Now, @Attribute);

		select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the author account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PriceTransactionId, @AuthorAccountId, @Price, null, Balance - @Price
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @AuthorAccountId);

		-- We are going to post two entries. Reuse the balance.
		select @InitialReviewerBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @ReviewerAccountId);

		-- Temporary credit the reviewer account with the full reward amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@PriceTransactionId, @ReviewerAccountId, null, @Price, @InitialReviewerBalance + @Price)

		-- Deduct service fee from the reviewer account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @ReviewerAccountId, @Fee, null, @InitialReviewerBalance + @Price - @Fee)

		-- Credit the service.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @FeeTransactionId, @RevenueAccountId, null, @Fee, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

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