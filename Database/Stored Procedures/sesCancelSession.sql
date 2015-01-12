
CREATE PROCEDURE [dbo].[sesCancelSession]
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

	declare @Start datetime2(2), @End datetime2(2), @HostUserId int, @GuestUserId int, @Price decimal(9,2), @GuestHasVacantTime bit,
		@OtherUserId int, @Attribute nvarchar(100), @MessageType char(6), @Now datetime2(2);

	declare @t table (
		Start datetime2(2),
		[End] datetime2(2),
		HostUserId int,
		GuestUserId int,
		Price decimal(9,2),
		ConfirmationTime datetime2(2),
		GuestHasVacantTime bit
	);

	set @Attribute = cast(@SessionId as nvarchar(100));

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

			update dbo.sesSessions 
				set CancellationTime = @Now, CancellationUserId = @UserId
			output deleted.Start, deleted.[End], deleted.HostUserId, deleted.GuestUserId, deleted.Price, deleted.ConfirmationTime, deleted.GuestHasVacantTime 
				into @t
			where Id = @SessionId
				and (HostUserId = @UserId or GuestUserId = @UserId)
				and CancellationTime is null
				and FinishTime is null				
				and ((HostUserId = @UserId) or (Start > @Now));

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to cancel session.', 16, 1, @ProcName, @UserId, @SessionId);
				
			select @Start = Start, @End = [End], @HostUserId = HostUserId, @GuestUserId = GuestUserId, 
				@Price = Price,	@GuestHasVacantTime = GuestHasVacantTime,
				@OtherUserId = iif(@UserId = HostUserId, GuestUserId, HostUserId),
				@MessageType = iif(@UserId = HostUserId, 'MSSSCH', 'MSSSCG')
			from @t;

			update dbo.sesScheduleEvents 
				set [Type] = 
					iif(@UserId = @HostUserId,
						iif(UserId = @HostUserId, 'SESSCS', 'SESSCO'),
						iif(UserId = @GuestUserId, 'SESSCS', 'SESSCO')
					)
			where Attribute = @Attribute
				and (UserId = @HostUserId or UserId = @GuestUserId)
				and dbo.sesIsSession([Type]) = 1;

			if @@rowcount != 2
				raiserror('%s,%d,%d:: Failed to change the event status.', 16, 1, @ProcName, @UserId, @SessionId);

			-- Return the session time as a vacant time to the host user. 
			exec dbo.sesInsertVacantTime @UserId = @HostUserId, @Start = @Start, @End = @End;

			-- We store the information about the guest user's vacant time prior to the session request in the session record.
			if (@GuestHasVacantTime = 1) begin
	
				exec dbo.sesInsertVacantTime @UserId = @GuestUserId, @Start = @Start, @End = @End;
				
			end
						
			exec dbo.appPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @OtherUserId, @Type = @MessageType, @Attribute = @Attribute, @ExtId = @MessageExtId;

			-- If Price is non-zero, refund and reward are settled at session closing.
			exec dbo.sesFinishSession @SessionId = @SessionId;

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
    ON OBJECT::[dbo].[sesCancelSession] TO [websiterole]
    AS [dbo];

