

CREATE PROCEDURE [dbo].[accPostIncomingPayPalPayment]
	@UserName nvarchar(200), -- 'UnknownPayer' can be substituted by the app.
	@Amount decimal(18, 2),
	@Fee decimal(18, 2), 
	@ExtId nvarchar(50), -- PayPal's Tx
	@ReceiptId nvarchar(100) = null, -- This value is displayd to the sender in the confirmation email from PayPal as "Receipt No"
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

	declare @PaymentTransactionId int, @FeeTransactionId int, @InitialUserBalance decimal(18,2), @InitialFeeBalance decimal(18,2);

	declare @UserId int = dbo.appGetUserId(@UserName);
	declare @UserAccountId int = dbo.accGetPersonalAccount(@UserId);
	declare @CashAccountId int = dbo.appGetConstantAsInt('Account.$Service.PayPalCash');
	declare @FeeAccountId int = dbo.appGetConstantAsInt('Account.$Service.IncomingPayPalPaymentFee');

	if @ExternalTran = 0
		begin transaction;

		-- We postpone creation of a user account until it is really needed.
		if (@UserAccountId is null) begin
		
			exec dbo.accCreateUserAccounts @UserId = @UserId;

			set @UserAccountId = dbo.accGetPersonalAccount(@UserId);

		end

		-- Do not rely on the FK check. Otherwise we can lose an autoinc PK value in dbo.accTransactions on a rollback.
		if (@UserAccountId is null) or (@CashAccountId is null) or (@FeeAccountId is null)
		begin
			raiserror('%s,%s:: Account not found.', 16, 1, @ProcName, @UserName);  
		end;

		-- ExtId is a PK. We use it as a guardian to avoid duplicate payment posting.
		insert dbo.accPostedPayPalPayments (ExtId)
			values (@ExtId);

		declare @Now datetime2(7) = sysutcdatetime();

		insert dbo.accTransactions ([Type], ObservedTime, Attribute, Details)
			values ('TRPPIP', @Now, @ReceiptId, @Details);

		select @PaymentTransactionId = scope_identity() where @@rowcount != 0;

		-- Make the user to compensate the fee.
		insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRPPIF', @Now, @ReceiptId);

		select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- We are going to post two entries. Reuse the balance.
		select @InitialUserBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId);

		select @InitialFeeBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @FeeAccountId);

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PaymentTransactionId, @CashAccountId, @Amount - @Fee, null, Balance + @Amount - @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @CashAccountId);
	
		-- Post the fee initially
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @FeeAccountId, @Fee, null, @InitialFeeBalance + @Fee);

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@PaymentTransactionId, @UserAccountId, null, @Amount, @InitialUserBalance + @Amount);

		-- Compensate
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @FeeAccountId, null, @Fee, @InitialFeeBalance);

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @UserAccountId, @Fee, null, @InitialUserBalance + @Amount - @Fee);

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

