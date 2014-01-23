


CREATE PROCEDURE [dbo].[accCreateAccount]
	@UserId int,
	@AccountTypeId char(4)
AS
BEGIN
/*
Creates a new money account for a user. Initializes zero balance on the account.

20121015 AF. Initial release.
20131001 AF. Reincarnation.
*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @IsDebit bit, @AccountId int, @TransactionId int;

	select @AccountTypeId = Id, @IsDebit = IsDebit from dbo.accAccountTypes where Id = @AccountTypeId;

	if @AccountTypeId is null or @IsDebit is null
	begin
		raiserror('%s:: One or more accounting constants not found.', 16, 1, @ProcName);  
	end;

	if @ExternalTran = 0
		begin transaction;

	insert dbo.accAccounts (AccountTypeId, UserId)
		values (@AccountTypeId, @UserId);

	select @AccountId = scope_identity() where @@rowcount != 0;

	insert dbo.accTransactions (TransactionTypeId)
		values ('NACC');

	select @TransactionId = scope_identity() where @@rowcount != 0;

	insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		values (@TransactionId, @AccountId, 
		--case when @IsDebit = 1 then 0 else null end, case when @IsDebit = 0 then 0 else null end,
		nullif(0, @IsDebit), nullif(@IsDebit, 1),
		0.0);

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