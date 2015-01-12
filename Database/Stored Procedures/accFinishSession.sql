
CREATE PROCEDURE [dbo].[accFinishSession]
	@SessionId int
AS
BEGIN
/*
HostUser - the user who offered and confirmed the session, who is payed.
GuestUser - the user who requested the session, who pays.

This procedure assumes that the Price of the session is non-zero.

It is called internally by dbo.sesCloseSession, so do not set access permitions on it.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @HostUserId int, @GuestUserId int, 
		@Start datetime2(2), @ConfirmationTime datetime2(2), @CancellationTime datetime2(2), @CancellationUserId int,
		@Price decimal(9,2), @Refund decimal(9,2), @Reward decimal(9,2), @ServiceFeeRate float, @Fee decimal(9,2),
		@HostAccountId int, @GuestAccountId int, @EscrowAccountId int, @RevenueAccountId int,
		@EscrowTransactionId int, @FeeTransactionId int, @InitialHostBalance decimal(18,2), 
		@Attribute nvarchar(100), @Now datetime2(2), @FinishDelay int;

	-- Give the users opportunity to dispute the session before finish it.
	set @FinishDelay = dbo.appGetConstantAsInt('Sessions.FinishDelay.Minutes');

	select @Price = Price, @HostUserId = HostUserId, @GuestUserId = GuestUserId, 
		@Start = Start, @ConfirmationTime = ConfirmationTime, @CancellationTime = CancellationTime, @CancellationUserId = CancellationUserId,
		@ServiceFeeRate = dbo.appGetFeeRate('TRSSFD', RequestTime, Price * 60 / datediff(minute, Start, [End]))
	from dbo.sesSessions 
	where Id = @SessionId
		and DisputeTimeByHost is null
		and DisputeTimeByGuest is null
		and FinishTime is null
		and (datediff(minute, [End], @Now) >= @FinishDelay	or CancellationTime is not null);

	-- Price is not nullable. This procedure assumes that the Price of the session is non-zero.
	if coalesce(@Price, 0) <= 0
		raiserror('%s,%d:: The session cannot be finished.', 16, 1, @ProcName, @SessionId);

	set @HostAccountId = dbo.accGetPersonalAccount(@HostUserId);

	select 
		@GuestAccountId = max(iif([Type] = 'ACPERS', Id, 0)),
		@EscrowAccountId = max(iif([Type] = 'ACESCR', Id, 0))
	from dbo.accAccounts
	where UserId = @GuestUserId;

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@HostAccountId is null) or (@GuestAccountId is null) or (@EscrowAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d,%d:: Account not found.', 16, 1, @ProcName, @HostUserId, @GuestUserId);

	-- CASE 1. The session was not confirmed by the host user, either cancelled by the guest user before that or idled uncancelled until closing.
	-- CASE 2. The session is cancelled by the host user. 
	-- Return the entire escrow amount back to the guest user.
	if (@ConfirmationTime is null or @CancellationUserId = @HostUserId) begin

		set @Refund = @Price;

	end

	-- CASE 3. Confirmed session cancelled by the guest user.
	-- Return the escrow amount proportional to the time period left until the start and inversely proportional to the period from the confirmation to the start.
	------|--------------Price---------------|----
	------|---- Reward -----|----- Refund ---|----
	--Confirmed-----CancelledByGuest-------Start--
	if (@ConfirmationTime is not null and @CancellationUserId = @GuestUserId) begin

		set @Refund = cast(
			@Price * datediff(minute, @CancellationTime, @Start) / datediff(minute, @ConfirmationTime, @Start)
			as decimal(9,2));

	end

	-- CASE 4. Confirmed uncancelled session.
	if (@ConfirmationTime is not null and @CancellationUserId = null) begin

		set @Refund = 0;

	end

	set @Reward = @Price - @Refund;

	set @Fee = cast((@Reward * @ServiceFeeRate) as decimal(9,2));

	set @Attribute = cast(@SessionId as nvarchar(100));

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		update dbo.sesSessions			 
		set FinishTime = @Now
		where Id = @SessionId
			and DisputeTimeByHost is null
			and DisputeTimeByGuest is null
			and FinishTime is null
			and (datediff(minute, [End], @Now) >= @FinishDelay	or CancellationTime is not null);

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to finish session.', 16, 1, @ProcName, @SessionId);

		insert into dbo.accTransactions ([Type], ObservedTime, Attribute)
		values ('TRSSFN', @Now, @Attribute);

		select @EscrowTransactionId = scope_identity() where @@rowcount != 0;

		-- If there is a Reward, than do a service fee transaction as well.
		if (@Reward > 0) begin

			insert dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRSSFD', @Now, @Attribute);

			select @FeeTransactionId = scope_identity() where @@rowcount != 0;

		end

		set transaction isolation level serializable;

		-- Debit the escrow account by the full Price amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @EscrowTransactionId, @EscrowAccountId, @Price, null, Balance - @Price
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @EscrowAccountId)
				and Balance >= @Price;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: Non-sufficient funds.', 16, 1, @ProcName, @GuestUserId, @SessionId);

		-- Distribute the Price amount.
		-- If there is a Refund
		if (@Refund > 0) begin

			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @EscrowTransactionId, @GuestAccountId, null, @Refund, Balance + @Refund
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @GuestAccountId);

		end

		-- If there is a Reward and a service fee.
		if (@Reward > 0) begin

			-- We are going to post two entries. Reuse the balance.
			select @InitialHostBalance = Balance
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @HostAccountId);

			-- First credit the hosts's account with the full Reward amount.
			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@EscrowTransactionId, @HostAccountId, null, @Reward, @InitialHostBalance + @Reward);

			-- Second deduct the service fee from the hosts's account.
			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			values (@FeeTransactionId, @HostAccountId, @Fee, null, @InitialHostBalance + @Reward - @Fee);

			-- Credit the service.
			insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
				select @FeeTransactionId, @RevenueAccountId, null, @Fee, Balance + @Fee
				from dbo.accEntries
				where Id = (select max(Id) from dbo.accEntries where AccountId = @RevenueAccountId);

		end

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