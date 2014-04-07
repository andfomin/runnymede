
CREATE PROCEDURE [dbo].[relCancelSession]
	@EventId int,
	@UserId int,
	@Message nvarchar(1000) = null
AS
BEGIN
/*
We refund the SecondUser the full price on whatever cancellation action.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Start smalldatetime, @Type nchar(4), @FirstUserId int, @SecondUserId int, @EscrowAmount decimal(18,2), 
		@RecipientUserId int, @Attribute nvarchar(100), @Text nvarchar(1000);
	
	-- The sign of @EscrowAmount determines the direction of the transfer.
	select @Start = Start, @Type = [Type], @FirstUserId = UserId, @SecondUserId = SecondUserId, @EscrowAmount = -Price,
		@RecipientUserId = case when UserId = @UserId then SecondUserId else UserId end
	from dbo.relScheduleEvents
	where Id = @EventId;

	if (@FirstUserId != @UserId and @SecondUserId != @UserId)
		raiserror('%s,%d,%d:: The user is not related to the session.', 16, 1, @ProcName, @UserId, @EventId);

	if @Type in ('CSUS', 'CSSU')
		raiserror('%s,%d:: The session is already cancelled.', 16, 1, @ProcName, @EventId);

	if ((@UserId = @SecondUserId) and (@Type = 'CFSN') and (@Start < sysutcdatetime()))
		raiserror('%s,%d,%d:: User cannot cancel a confirmed session after it has started.', 16, 1, @ProcName, @UserId, @EventId);

	set @Attribute = cast(@EventId as nvarchar(100));

	set @Text = 'SESSION CANCELLED. ' + coalesce(@Message, '');

	if @ExternalTran = 0
		begin transaction;

			update dbo.relScheduleEvents 
			set [Type] = case when UserId = @UserId then 'CSUS' else 'CSSU' end,
		    CancellationTime = sysutcdatetime()
			where Id = @EventId
				and (UserId = @UserId or SecondUserId = @UserId)
				and [Type] in ('RQSN', 'CFSN')
				and CancellationTime is null;

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to cancel session.', 16, 1, @ProcName, @UserId, @EventId);

			if @EscrowAmount is not null
				exec dbo.accChangeEscrow @SecondUserId, @EscrowAmount, 'SNCN', @Attribute, null;		
			
			exec dbo.relPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @RecipientUserId, @Type = 'SSSN', @Attribute = @Attribute, @Text = @Text;

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
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[relCancelSession] TO [websiterole]
    AS [dbo];

