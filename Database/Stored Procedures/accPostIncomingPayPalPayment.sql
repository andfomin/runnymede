

CREATE PROCEDURE [dbo].[accPostIncomingPayPalPayment]
	--@UserName nvarchar(200) = null,
	@UserId int = null,
	@ExtId nvarchar(50), -- PayPal's Transaction ID a.k.a. Tx
	@Amount decimal(9, 2),
	@Fee decimal(9, 2), 
	@Tax decimal(9, 2),
	@Details xml = null
AS
BEGIN
/*
Post accounting entries on an incoming PayPal payment.
20121016 AF. Initial release.
20150129 AF. Do not make the customer to compensate the fee. The service will absorb the fee.
20150227 AF. Deduct the payment fee again.
20150324 AF. Parameters order changed. @ReceiptId removed, @TaxAmount added. 
Making the user to compensate the fee does not make sence, in such case we cannot post the fee as an expence and the bottom line for income tax is the same as if we posted the fee as an expence.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @PaymentTransactionId int, @TaxTransactionId int;

	-- The real user not found.
	if (coalesce(@UserId, 0) = 0)
		set @UserId = dbo.appGetUserId('$UnknownPayPalPayer');

	exec dbo.accCreateAccountsIfNotExist @UserId = @UserId, @Types = N'<Types><Type>ACUCSH</Type><Type>ACUESC</Type></Types>';

	declare @UserAccountId int =  dbo.accGetUserCashAccount(@UserId);
	declare @PayPalAccountId int = dbo.appGetConstantAsInt('Account.$Service.PayPalCash');
	declare @FeeAccountId int = dbo.appGetConstantAsInt('Account.$Service.IncomingPayPalPaymentFee');
	declare @TaxAccountId int = dbo.appGetConstantAsInt('Account.$Service.IncomingPayPalPaymentSalesTax');

	-- Do not rely on the FK check. Otherwise we can lose an autoinc PK value in dbo.accTransactions on a rollback.
	if (@UserAccountId is null) or (@PayPalAccountId is null) or (@FeeAccountId is null) or (@TaxAccountId is null)
	begin
		raiserror('%s,%d:: Account not found.', 16, 1, @ProcName, @UserId);  
	end;

	if @ExternalTran = 0
		begin transaction;

		-- ExtId is a PK. We use it as a guardian to avoid duplicate payment posting.
		insert dbo.accPostedPayPalPayments (ExtId)
		values (@ExtId);

		declare @Now datetime2(2) = sysutcdatetime();

		insert dbo.accTransactions ([Type], ObservedTime, Attribute, Details)
		values ('TRPPIP', @Now, @ExtId, @Details);

		select @PaymentTransactionId = scope_identity() where @@rowcount != 0;

		-- PayPal adds sales tax to payments from Canadian residents (We indicated that in the PayPal settings)
		if (coalesce(@Tax, 0) != 0) begin

			insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRPPIT', @Now, @ExtId);

			select @TaxTransactionId = scope_identity() where @@rowcount != 0;

		end

		set transaction isolation level serializable;

		-- Debit the cash account with the net amount
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PaymentTransactionId, @PayPalAccountId, @Amount - @Fee, null, Balance + @Amount - @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @PayPalAccountId);
	
		-- Credit the user account with the gross amount the user paid
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PaymentTransactionId, @UserAccountId, null, @Amount, Balance + @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId);

		-- Write down the transfer fee as an expence.
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PaymentTransactionId, @FeeAccountId, @Fee, null, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @FeeAccountId);

		-- Deduct sales tax for Canadians. This case is going to be rare, so we do not bother with reusing the initial balance.
		if (coalesce(@Tax, 0) != 0) begin
		
			insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @TaxTransactionId, @UserAccountId, @Tax, null, Balance - @Tax
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId);

			insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @TaxTransactionId, @TaxAccountId, null, @Tax, Balance + @Tax
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @TaxAccountId);

		end

/* Remark. Although to post two rows in a single insert seems more performant, we cannot guarantee the order of the rows in that case. 
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

