

CREATE PROCEDURE [dbo].[accPostIncomingPayPalPayment]
	@UserName nvarchar(200) = null,
	@Amount decimal(18, 2),
	@Fee decimal(18, 2), 
	@ExtId nvarchar(50), -- PayPal's Transaction ID a.k.a. Tx
	@ReceiptId nvarchar(100) = null, -- This value is displayd to the sender in the confirmation email from PayPal as "Receipt No"
	@Details xml = null
AS
BEGIN
/*
Post accounting entries on an incoming PayPal payment.
20121016 AF. Initial release.

20150129 AF. Do not make the customer to compensate the fee. The service will absorb the fee.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @TransactionId int;

	declare @UserId int = dbo.appGetUserId(@UserName);

	-- The real user not found.
	if (@UserId is null)
		set @UserId = dbo.appGetUserId('$UnknownPayPalPayer');

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

		insert dbo.accTransactions ([Type], ObservedTime, Attribute, Details)
			values ('TRPPIP', sysutcdatetime(), @ReceiptId, @Details);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @CashAccountId, @Amount - @Fee, null, Balance + @Amount - @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @CashAccountId);
	
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @FeeAccountId, @Fee, null, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @FeeAccountId);

		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @UserAccountId, null, @Amount, Balance + @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId);

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

