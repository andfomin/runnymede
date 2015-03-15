
CREATE PROCEDURE [dbo].[accFinishSession]
	@SessionId int,
	@FinishTime datetime2(2) output
AS
BEGIN
/*
It is called internally by dbo.sesCloseSession, so do not set access permitions on it.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @LearnerUserId int, @Price decimal(9,2), @EscrowAccountId int, @RevenueAccountId int, @TransactionId int; 

	select @LearnerUserId = LearnerUserId, @Price = Price
	from dbo.sesSessions 
	where Id = @SessionId
		and ClosingTime is null
		and (
			-- Give the learner an opportunity to dispute the session before it is closed programmatically.
			datediff(minute, [End], sysutcdatetime()) >= dbo.appGetConstantAsInt('Sessions.ClosingDelay.Minutes') 
			or Rating is not null				
		);

	-- WE assume that the Price of the session must be non-zero.
	if coalesce(@Price, 0) <= 0
		raiserror('%s,%d:: The session cannot be finished.', 16, 1, @ProcName, @SessionId);

	set @EscrowAccountId = dbo.accGetEcrowAccount(@LearnerUserId);

	set @RevenueAccountId = dbo.appGetConstantAsInt('Account.$Service.ServiceRevenue');

	if (@EscrowAccountId is null) or (@RevenueAccountId is null)
		raiserror('%s,%d:: Account not found.', 16, 1, @ProcName, @LearnerUserId);

	set @FinishTime = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		insert into dbo.accTransactions ([Type], ObservedTime, Attribute)
			values ('TRSSCL', @FinishTime, cast(@SessionId as nvarchar(100)));

		select @TransactionId = scope_identity() where @@rowcount != 0;

		set transaction isolation level serializable;

		-- Debit the escrow account for the full Price amount.
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @EscrowAccountId, @Price, null, Balance - @Price
			from dbo.accEntries
			where Id = (select max(Id) from dbo.accEntries where AccountId = @EscrowAccountId)
				and Balance >= @Price;

		if @@rowcount = 0
			raiserror('%s,%d,%d:: Non-sufficient funds.', 16, 1, @ProcName, @LearnerUserId, @SessionId);

		-- Credit the service for the full Price amount
		insert into dbo.accEntries (TransactionId, AccountId, Debit, Credit, Balance)
			select @TransactionId, @RevenueAccountId, null, @Price, Balance + @Price
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