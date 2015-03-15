
CREATE PROCEDURE [dbo].[accFinishReview]
	@ReviewId int,
	@UserId int,
	@FinishTime datetime2(2) output
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

	declare @AuthorUserId int, @Price decimal(9,2), @Attribute nvarchar(100),
		@AuthorAccountId int, @RevenueAccountId int, @TransactionId int;

	select @Price = R.Price, @AuthorUserId = E.UserId
	from dbo.exeReviews R
		inner join dbo.exeExercises E on R.ExerciseId = E.Id
	where R.Id = @ReviewId
		and R.UserId = @UserId 
		and R.StartTime is not null
		and R.FinishTime is null;

	-- Price is not nullable.
	if coalesce(@Price, -1) < 0
		raiserror('%s,%d:: The review cannot be finished.', 16, 1, @ProcName, @ReviewId);

	set @AuthorAccountId = dbo.accGetEcrowAccount(@AuthorUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	set @Attribute = convert(nvarchar(100), @ReviewId);

	set @FinishTime = sysutcdatetime();

	if (@Price > 0) begin

		if (@AuthorAccountId is null) or (@RevenueAccountId is null)
			raiserror('%s,%d,%d:: Account not found.', 16, 1, @ProcName, @AuthorUserId, @UserId);

		if @ExternalTran = 0
			begin transaction;

			insert dbo.accTransactions ([Type], ObservedTime, Attribute)
				values ('TRRVFN', @FinishTime, @Attribute);

			select @TransactionId = scope_identity() where @@rowcount != 0;

			set transaction isolation level serializable;

			-- Debit the author's escrow account.
			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @TransactionId, @AuthorAccountId, @Price, null, Balance - @Price
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @AuthorAccountId);

			-- Credit the service.
			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @TransactionId, @RevenueAccountId, null, @Price, Balance + @Price
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

		if @ExternalTran = 0
			commit;

	end

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