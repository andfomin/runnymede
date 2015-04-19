


CREATE PROCEDURE [dbo].[accBuyReviewsOnIncomingPayPalPayment]
	@UserName nvarchar(200) = null,
	@ExtId nvarchar(50), -- PayPal's Transaction ID a.k.a. Tx
	@Amount decimal(9, 2),
	@Details xml = null
AS
BEGIN
/*
20150417 AF. Initial release.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Quantity int, @TransactionId int;

	declare @ReviewPacks xml = dbo.appGetValueAsXml('VLRVPK');

	select @Quantity = T.C.value('@quantity[1]', 'int')
	from @ReviewPacks.nodes('/Packs/Pack') T(C)
	where T.C.value('@totalPrice[1]', 'decimal(9,2)') = @Amount;

	if (@Quantity is null)
	begin
		raiserror('%s,%s,%s:: Review pack not found.', 16, 1, @ProcName, @UserName, @ExtId);  
	end;

	declare @UserId int = dbo.appGetUserId(@UserName);
	declare @UserAccountId int = dbo.accGetPersonalAccount(@UserId);
	declare @ReviewAccountId int = dbo.accGetReviewAccount(@UserId);
	declare @RevenueAccountId int = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@UserAccountId is null) or (@ReviewAccountId is null) or (@RevenueAccountId is null)
	begin
		raiserror('%s,%s:: Account not found.', 16, 1, @ProcName, @UserName);  
	end;

	if @ExternalTran = 0
		begin transaction;

		insert dbo.accTransactions ([Type], ObservedTime, Attribute, Details)
			values ('TRPRRV', sysutcdatetime(), @ExtId, @Details);

		select @TransactionId = scope_identity() where @@rowcount != 0;

		-- Debit the user account for the amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @UserAccountId, @Amount, null, Balance - @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @UserAccountId)
				and Balance >= @Amount;

		if @@rowcount = 0
			raiserror('%s,%d:: Non-sufficient funds.', 16, 1, @ProcName, @UserAccountId);

		-- Credit the service for the amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @RevenueAccountId, null, @Amount, Balance + @Amount
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

		-- Credit the user's review account with the review quantity.
		insert dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @ReviewAccountId, null, @Quantity, Balance + @Quantity
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @ReviewAccountId);
	
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
    ON OBJECT::[dbo].[accBuyReviewsOnIncomingPayPalPayment] TO [websiterole]
    AS [dbo];

