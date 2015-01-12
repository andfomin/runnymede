
CREATE PROCEDURE [dbo].[sesDisputeSession]
	@SessionId int,
	@UserId int,
	@MessageExtId nchar(12) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Attribute nvarchar(100), @OtherUserId int, @MessageType char(6);

	declare @t table (
		HostUserId int,
		GuestUserId int
	);

	set @Attribute = cast(@SessionId as nvarchar(100));

	declare @Now datetime2(2) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

			-- A user can dispute only during a certain period of time after the session.
			-- There is no point disputing a session if the price is 0.
			update dbo.sesSessions set 
				DisputeTimeByHost = iif(HostUserId = @UserId, coalesce(DisputeTimeByHost, @Now), DisputeTimeByHost),
				DisputeTimeByGuest = iif(GuestUserId = @UserId, coalesce(DisputeTimeByGuest, @Now), DisputeTimeByGuest)
			output deleted.HostUserId, deleted.GuestUserId into @t
			where Id = @SessionId
				and (HostUserId = @UserId or GuestUserId = @UserId)
				and Start < @Now		
				and CancellationTime is null
				and FinishTime is null		
				and datediff(minute, [End], @Now) < dbo.appGetConstantAsInt('Sessions.ClosingDelay')
				and Price > 0;

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to dispute session.', 16, 1, @ProcName, @UserId, @SessionId);
				
			select 
				@OtherUserId = iif(@UserId = HostUserId, GuestUserId, HostUserId),
				@MessageType = iif(@UserId = HostUserId, 'MSSSDH', 'MSSSDG')
			from @t;
							
			exec dbo.appPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @OtherUserId, @Type = @MessageType, @Attribute = @Attribute, @ExtId = @MessageExtId;

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