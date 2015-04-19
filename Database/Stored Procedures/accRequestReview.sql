
create PROCEDURE [dbo].[accRequestReview]
	@UserId int,
	@Attribute nvarchar(100),
	@Now datetime2(2) output
AS
BEGIN
/*
20150417 AF.
It is called internally by dbo.exeCreateReviewRequest, so do not set access permitions on it.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @UserAccountId int, @ServiceAccountId int, @TransactionId int;

	set @UserAccountId = dbo.accGetReviewAccount(@UserId);

	set @ServiceAccountId = dbo.appGetConstantAsInt('Account.$Service.RequestedReviews');

	if (@UserAccountId is null) or (@ServiceAccountId is null)
		raiserror('%s,%d:: Account not found.', 16, 1, @ProcName, @UserId);

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRRVRQ', @Now, @Attribute);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the user's review account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @UserAccountId, 1, null, Balance - 1
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId)
				and Balance > 0;

		if @@rowcount = 0
			raiserror('%s,%d:: Not enough purchased reviews.', 16, 1, @ProcName, @UserAccountId);

		-- Credit the service review account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @ServiceAccountId, null, 1, Balance + 1
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @ServiceAccountId);

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