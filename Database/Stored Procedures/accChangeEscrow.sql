



CREATE PROCEDURE [dbo].[accChangeEscrow]
	@UserId int,
	@Amount decimal(9, 2), -- The sign of @Amount determines the direction of the transfer. A negative amount means refund.
	@TransactionType char(6),
	@Attribute nvarchar(100) = null,
	@Details xml = null,
	@Now datetime2(2) output
AS
BEGIN
/*
Transfer user's money from the personal account to the escrow account on review request.
Or transfer vice versa on review request cancelation.

The sign of @Amount determines the direction of the transfer. 
A positive @Amount means from the user to the escrow, a negative @Amount means from the escrow to the user.

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
		@PersonalAccountId = max(iif([Type] = 'ACUCSH', Id, 0)),
		@EscrowAccountId = max(iif([Type] = 'ACUESC',Id, 0))
	from dbo.accAccounts
	where UserId = @UserId;

	if (nullif(@PersonalAccountId, 0) is null) or (nullif(@EscrowAccountId, 0) is null)
		raiserror('%s,%d:: Account not found.', 16, 1, @ProcName, @UserId);  

	if @Amount >= 0
		select @DebitedAccountId = @PersonalAccountId, @CreditedAccountId = @EscrowAccountId;
	else
		select @DebitedAccountId = @EscrowAccountId, @CreditedAccountId = @PersonalAccountId;

	-- dbo.accTransactions.TransactionType is a FK. The value comes from our upstream code.
	--if not exists (select * from dbo.accTransactionTypes where Id = @TransactionType)
	--	raiserror('%s,%s:: Transaction type not found.', 16, 1, @ProcName, @TransactionType); 

	select @Balance = Balance
	from dbo.accEntries
	where Id = (select max(Id) from dbo.accEntries where AccountId = @DebitedAccountId)

	if coalesce(@Balance, 0) < abs(@Amount) 
		raiserror('%s,%d:: Non-sufficient funds.', 16, 1, @ProcName, @UserId); 

	set @Now = sysutcdatetime();
			 
	if @ExternalTran = 0
		begin transaction;

		insert into dbo.accTransactions ([Type], ObservedTime, Attribute, Details)
			values (@TransactionType, @Now, @Attribute, @Details);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @DebitedAccountId, abs(@Amount), null, Balance - abs(@Amount)
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @DebitedAccountId);

		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @CreditedAccountId, null, abs(@Amount), Balance + abs(@Amount)
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @CreditedAccountId);

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