


CREATE PROCEDURE [dbo].[accCreateAccount]
	@UserId int,
	@Type char(6)
AS
BEGIN
/*
Creates a new money account for a user. Initializes zero balance on the account.

20121015 AF. Initial release.
20131001 AF. Reincarnation.
20141228 AF Migration from dbo.accAccountTypes to dbo.appTypes
*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @IsDebit bit, @AccountId int, @TransactionId int;

	select @IsDebit = C.value('.', 'bit')
	from (
		select Id, convert(xml, [Description]) as [Description]
		from dbo.appTypes
		where Id = @Type
	) q
		cross apply q.[Description].nodes('/IsDebit') T(C);

	if @IsDebit is null
	begin
		raiserror('%s:: Account property not found.', 16, 1, @ProcName);  
	end;

	if @ExternalTran = 0
		begin transaction;

	insert dbo.accAccounts ([Type], UserId)
		values (@Type, @UserId);

	select @AccountId = scope_identity() where @@rowcount != 0;

	insert dbo.accTransactions ([Type])
		values ('TRNACC');

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