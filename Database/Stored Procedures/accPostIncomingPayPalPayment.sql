

CREATE PROCEDURE [dbo].[accPostIncomingPayPalPayment]
	@UserName nvarchar(200), -- 'UnknownPayer' can be substituted by the app.
	@Amount decimal(18, 2),
	@Fee decimal(18, 2), 
	@ExtId nvarchar(50), -- PayPal's Tx
	@Details xml = null
AS
BEGIN
/*
Adds entries on an incoming PayPal payment.

20121016 AF. Initial release.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @UserId int = dbo.appGetUserId(@UserName);
	declare @UserAccountId int = dbo.accGetPersonalAccount(@UserId);
	declare @CashAccountId int = dbo.appGetConstantAsInt('Account.$Service.PayPalCash');
	declare @FeeAccountId int = dbo.appGetConstantAsInt('Account.$Service.IncomingPayPalPaymentFee');

	declare @TransactionId int;

	--select @PaymentTransactionTypeId = Id from dbo.accounting_TransactionTypes where Name = 'INCOMING_PAYMENT';
	--select @FeeTransactionTypeId = Id from dbo.accounting_TransactionTypes where Name = 'INCOMING_PAYMENT_FEE_COMPENSATION';

	-- Do not rely on the FK check. Otherwise we can lose an autoinc PK value in [Accounting].[Transactions] on a rollback.
	if (@UserAccountId is null) or (@CashAccountId is null) or (@FeeAccountId is null)
	begin
		raiserror('%s,%s:: The account for the user not found.', 16, 1, @ProcName, @UserName);  
	end;

	declare @Now datetime2(7) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		-- ExtId is a PK. We use it as a guardian to avoid duplicate payment posting.
		insert dbo.accPostedPayPalPayments (ExtId)
			values (@ExtId);

		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Details)
			values ('PPIP', @Now, @Details);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @TransactionId, @UserAccountId, null, @Amount, E.Balance + @Amount
			from dbo.accEntries E
			where E.AccountId = @UserAccountId
			order by E.Id desc

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @TransactionId, @CashAccountId, @Amount - @Fee, null, E.Balance + @Amount - @Fee
			from dbo.accEntries E
			where E.AccountId = @CashAccountId
			order by E.Id desc
	
		-- Post the fee
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select top(1) @TransactionId, @FeeAccountId, @Fee, null, E.Balance + @Fee
			from dbo.accEntries E
			where E.AccountId = @FeeAccountId
			order by E.Id desc

		/* We absorb the PayPal fee. We assume the learner can only pay to a tutor. We deduct our combined service fee on money withdrawal by the tutor. */
		------ Make the user to compensate the fee.
		----insert dbo.accTransactions (TransactionTypeId, ObservedTime)
		----	values ('PPIF', @Now);

		----select @TransactionId = scope_identity() where @@rowcount != 0;

		----insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		----	select top(1) @TransactionId, @UserAccountId, @Fee, null, E.Balance - @Fee
		----	from dbo.accEntries E
		----	where E.AccountId = @UserAccountId
		----	order by E.Id desc

		----insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
		----	select top(1) @TransactionId, @FeeAccountId, null, @Fee, E.Balance - @Fee
		----	from dbo.accEntries E
		----	where E.AccountId = @FeeAccountId
		----	order by E.Id desc

/* Although to post two rows in a single insert seems more performant, we cannot guarantee the order of the rows in that case. 
We need to post entries in a particlar order to keep the running balance correct. */
	
	if @ExternalTran = 0
		commit;
end try
begin catch
	set @xstate = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback
	if @XState = -1
		rollback;
	throw;
end catch

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accPostIncomingPayPalPayment] TO [websiterole]
    AS [dbo];

