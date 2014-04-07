
CREATE PROCEDURE [dbo].[relDisputeSession]
	@EventId int,
	@UserId int,
	@Message nvarchar(1000) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @End smalldatetime, @Type nchar(4),	@RecipientUserId int, @Attribute nvarchar(100), @Text nvarchar(1000);

	declare @DisputeTimeWindow int = 121;
	
	-- The sign of @EscrowAmount determines the direction of the transfer.
	select @RecipientUserId = UserId
	from dbo.relScheduleEvents
	where Id = @EventId
		and SecondUserId = @UserId
		and [Type] = 'CFSN'
		and ClosingTime is null
		and [End] < sysutcdatetime()
		and [End] > dateadd(minute, -@DisputeTimeWindow, sysutcdatetime());

	if @RecipientUserId is null
		raiserror('%s,%d,%d:: The session cannot be disputed at the current conditions.', 16, 1, @ProcName, @EventId, @UserId);

	set @Attribute = cast(@EventId as nvarchar(100));

	set @Text = 'SESSION DISPUTED. ' + coalesce(@Message, '');

	if @ExternalTran = 0
		begin transaction;

			update dbo.relScheduleEvents 
			set [Type] = 'DSSN'
			where Id = @EventId
				and SecondUserId = @UserId
				and [Type] = 'CFSN'
				and ClosingTime is null
				and [End] < sysutcdatetime()
				and [End] > dateadd(minute, -@DisputeTimeWindow, sysutcdatetime());

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to dispute session.', 16, 1, @ProcName, @UserId, @EventId);
		
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
    ON OBJECT::[dbo].[relDisputeSession] TO [websiterole]
    AS [dbo];

