
CREATE PROCEDURE [dbo].[accCloseSession]
	@EventId int
AS
BEGIN
/*
FirstUser - the user who offered and performed session. Who is payed.
SecondUser - the user who requested and consumed session. Who pays.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @PayeeUserId int, @PayerUserId int, @PayeeAccountId int, @PayerAccountId int,  @RevenueAccountId int,
		@Price decimal(18,2), @ServiceFeeRate float, @Fee decimal(18,2),		
		@PaymentTransactionId int, @FeeTransactionId int, @InitialPayeeBalance decimal(18,2), @Attribute nvarchar(100);

	select @Price = Price, @PayeeUserId = UserId, @PayerUserId = SecondUserId
	from dbo.relScheduleEvents 
	where Id = @EventId
		and [Type] = 'CFSN'
		and ClosingTime is null
		and [End] < sysutcdatetime()

	if @Price is null
		raiserror('%s,%d:: Session cannot be closed.', 16, 1, @ProcName, @EventId);

	set @PayeeAccountId = dbo.accGetPersonalAccount(@PayeeUserId);

	set @PayerAccountId = dbo.accGetEcrowAccount(@PayerUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@PayeeAccountId is null) or (@PayerAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d,%d:: One or more accounts not found.', 16, 1, @ProcName, @PayeeUserId, @PayerUserId);

	set @ServiceFeeRate = dbo.appGetConstantAsFloat('Relationships.Sessions.ServiceFeeRate');
	set @Fee = cast((@Price * @ServiceFeeRate) as decimal(18,2));

	set @Attribute = cast(@EventId as nvarchar(100));

	declare @Now datetime2(7) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.relScheduleEvents 
		set [Type] = 'PYSN', ClosingTime = @Now
		where Id = @EventId
			and [Type] = 'CFSN'
			and ClosingTime is null
			and [End] < sysutcdatetime();

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to close session.', 16, 1, @ProcName, @EventId);

		-- Transfer reward.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('SNPY', @Now, @Attribute);

		select @PaymentTransactionId = scope_identity() where @@rowcount != 0;

		-- Deduct service fee.
		insert dbo.accTransactions (TransactionTypeId, ObservedTime, Attribute)
			values ('SNFD', @Now, @Attribute);

		select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the Payer's account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @PaymentTransactionId, @PayerAccountId, @Price, null, Balance - @Price
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @PayerAccountId);

		-- We are going to post two entries. Reuse the balance.
		select @InitialPayeeBalance = Balance
		from dbo.accEntries
		where Id = (select max(Id) from dbo.accEntries where AccountId = @PayeeAccountId);

		-- Temporary credit the Payee's account with the full price amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@PaymentTransactionId, @PayeeAccountId, null, @Price, @InitialPayeeBalance + @Price)

		-- Deduct the service fee from the Payee's account.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @PayeeAccountId, @Fee, null, @InitialPayeeBalance + @Price - @Fee)

		-- Credit the service.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @FeeTransactionId, @RevenueAccountId, null, @Fee, Balance + @Fee
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

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