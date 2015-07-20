
CREATE PROCEDURE [dbo].[accPostRevenue]
	@UserId int,
	@Amount decimal(9,2),
	@TransactionType char(6),
	@Attribute nvarchar(100),
	@Now datetime2(2) output
AS
BEGIN
/*
20150325 AF.
It is called internally by dbo., so do not set access permitions on it.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @EscrowAccountId int, @RevenueAccountId int, @TransactionId int;

	select @EscrowAccountId = Id 
	from dbo.accAccounts
	where UserId = @UserId 
		and [Type] = 'ACUESC';

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@EscrowAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d:: Account not found.', 16, 1, @ProcName, @UserId);

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values (@TransactionType, @Now, @Attribute);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the user's escrow account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @EscrowAccountId, @Amount, null, Balance - @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @EscrowAccountId)
				and Balance >= @Amount;

		if @@rowcount = 0
			raiserror('%s,%d:: Non sufficient funds.', 16, 1, @ProcName, @EscrowAccountId);

		-- Credit the service revenue.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @RevenueAccountId, null, @Amount, Balance + @Amount
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