



CREATE PROCEDURE [dbo].[accChangeEscrow]
	@UserId int,
	@Amount decimal(18, 2),
	@TransactionTypeId char(4),
	@Attribute int = null,
	@Details xml = null
AS
BEGIN
/*
Transfer user's money from the personal account to the escrow account on review request.
Or transfer vice versa on review request cancelation.
The sign of @Amount determines the direction of the transfer. 

20121113 AF. Initial release
*/
SET NOCOUNT ON;

declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @PersonalAccountId int, @EscrowAccountId int, @DebitedAccountId int, @CreditedAccountId int, 
		@TransactionId int, @Balance decimal(18,2);

	select 
		@PersonalAccountId = Max(case when AccountTypeId = 'PERS' then Id else 0 end),
		@EscrowAccountId = Max(case when AccountTypeId = 'ESCR' then Id else 0 end)
	from dbo.accAccounts
	where UserId = @UserId;

	if (nullif(@PersonalAccountId, 0) is null) or (nullif(@EscrowAccountId, 0) is null)
		raiserror('%s,%d:: The account for the user not found.', 16, 1, @ProcName, @UserId);  

	if @Amount >= 0
		select @DebitedAccountId = @PersonalAccountId, @CreditedAccountId = @EscrowAccountId;
	else
		select @DebitedAccountId = @EscrowAccountId, @CreditedAccountId = @PersonalAccountId;

	if not exists (select * from dbo.accTransactionTypes where Id = @TransactionTypeId)
		raiserror('%s,%s:: Transaction type not found.', 16, 1, @ProcName, @TransactionTypeId); 

	select top(1) @Balance = Balance
	from dbo.accEntries
	where AccountId = @DebitedAccountId
	order by Id desc;

	if @Balance is null or (@Balance < abs(@Amount)) 
		raiserror('%s,%d:: Non-sufficient funds.', 16, 1, @ProcName, @UserId); 
			 
	if @ExternalTran = 0
		begin transaction;

		insert into dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute, Details)
			values (@TransactionTypeId, sysutcdatetime(), @Attribute, @Details);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @TransactionId, @DebitedAccountId, abs(@Amount), null, E.Balance - abs(@Amount)
			from dbo.accEntries E
			where E.AccountId = @DebitedAccountId
			order by E.Id desc;

		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @TransactionId, @CreditedAccountId, null, abs(@Amount), E.Balance + abs(@Amount)
			from dbo.accEntries E
			where E.AccountId = @CreditedAccountId
			order by E.Id desc;

	if @ExternalTran = 0
		commit;
end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback
	if @XState = -1
		rollback;
	throw;
end catch



END