
CREATE PROCEDURE [dbo].[sesConfirmSession]
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

	declare @GuestUserId int, @Attribute nvarchar(100);

	declare @t table (
		GuestUserId int
	);

	-- The host must enter the Skype name in the profile
	if exists (
		select *
		from dbo.appUsers
		where Id = @UserId
			and nullif(SkypeName, '') is null
	)
		raiserror('%s,%d:: Please enter your Skype name on the Profile page.', 16, 1, @ProcName, @UserId);

	set @Attribute = cast(@SessionId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;

			update dbo.sesSessions 
			set ConfirmationTime = sysutcdatetime()
			output deleted.GuestUserId into @t
			where Id = @SessionId
				and HostUserId = @UserId
				and ConfirmationTime is null
				and CancellationTime is null
				and [End] > sysutcdatetime();

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to confirm session request.', 16, 1, @ProcName, @UserId, @SessionId);
				
			select @GuestUserId = GuestUserId from @t;

			update dbo.sesScheduleEvents 
				set [Type] = 'SESSCF'
			where Attribute = @Attribute
				and [Type] = 'SESSRQ';

			if @@rowcount != 2
				raiserror('%s,%d,%d:: Failed to change the event status.', 16, 1, @ProcName, @UserId, @SessionId);
				
			exec dbo.appPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @GuestUserId, @Type = 'MSSSCF', @Attribute = @Attribute, @ExtId = @MessageExtId;

			exec dbo.friUpdateLastContact @UserId = @UserId, @FriendUserId = @GuestUserId, @ContactType = 'CN__SF';

			exec dbo.friUpdateLastContact @UserId = @GuestUserId, @FriendUserId = @UserId, @ContactType = 'CN__SU';


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
    ON OBJECT::[dbo].[sesConfirmSession] TO [websiterole]
    AS [dbo];

