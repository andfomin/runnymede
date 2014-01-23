

CREATE PROCEDURE [dbo].[accCreateUserAccounts]
	@UserId int
AS
BEGIN
/*
Creates new money accounts for a user. Initializes zero balance on the accounts.

20121114 AF. Initial release.
20131001 AF. Reincarnation.
*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @PersonalAccountId int, @EscrowAccountId int, @TransactionId int;

	--select @PersonalAccountTypeId = Max(case when AT.Name = N'Personal' then AT.Id else 0 end)
	--	, @EscrowAccountTypeId = Max(case when AT.Name = N'Escrow' then AT.Id else 0 end)
	--from dbo.accAccountTypes AT

	--select @TransactionTypeId = Id from dbo.accTransactionTypes where Name = N'NEW_ACCOUNT';

	--if (nullif(@PersonalAccountTypeId, 0) is null) or (nullif(@EscrowAccountTypeId, 0) is null) or (@TransactionTypeId is null) 
	--begin
	--	raiserror('%s: One or more accounting constants not found.', 16, 1, @ProcName);  
	--end;

	if @ExternalTran = 0
		begin transaction;

	insert dbo.accAccounts (AccountTypeId, UserId)
		values ('PERS', @UserId);

	select @PersonalAccountId = Id from dbo.accAccounts where Id = scope_identity() and @@rowcount != 0;

	insert dbo.accAccounts (AccountTypeId, UserId)
		values ('ESCR', @UserId);

	select @EscrowAccountId = Id from dbo.accAccounts where Id = scope_identity() and @@rowcount != 0;

	insert into dbo.accTransactions (TransactionTypeId, ObservedTime)
		values ('NACC', sysutcdatetime());

	select @TransactionId = Id from dbo.accTransactions where Id = scope_identity() and @@rowcount != 0;

	insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		values (@TransactionId, @PersonalAccountId, null, 0.0, 0.0);

	insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		values (@TransactionId, @EscrowAccountId, null, 0.0, 0.0);

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