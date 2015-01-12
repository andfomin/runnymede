

CREATE PROCEDURE [dbo].[accCreateUserAccounts]
	@UserId int
AS
BEGIN
/*
Creates new money accounts for a user. Initializes zero balance on the accounts.

20121114 AF. Initial release.
20131001 AF. Reincarnation.
20141229 AF. Refactoring
*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @PersonalAccountId int, @EscrowAccountId int, @TransactionId int;

	if @ExternalTran = 0
		begin transaction;

	insert dbo.accAccounts ([Type], UserId)
		values ('ACPERS', @UserId);

	select @PersonalAccountId = scope_identity() where @@rowcount != 0;

	insert dbo.accAccounts ([Type], UserId)
		values ('ACESCR', @UserId);

	select @EscrowAccountId = scope_identity() where @@rowcount != 0;

	insert into dbo.accTransactions ([Type], ObservedTime)
		values ('TRNACC', sysutcdatetime());

	select @TransactionId = scope_identity() where @@rowcount != 0;

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