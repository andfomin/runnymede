
CREATE PROCEDURE [dbo].[sesRequestSession]
	@HostUserId int, -- who will confirm
	@GuestUserId int, -- who requests
	@Start smalldatetime,
	@End smalldatetime,
	@Price decimal(9,2) = 0,
	@MessageExtId nchar(12) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SessionId int, @Attribute nvarchar(100), @Now datetime2(2), @GuestVacantTimeFound bit, @CalculatedPrice decimal(9,2);

	set @Now = sysutcdatetime(); 

	--if not exists (select * from dbo.appUsers where Id = @UserId and IsTeacher = 1)
	--	raiserror('%s,%d:: The user is not a teacher.', 16, 1, @ProcName, @UserId);

	if not exists (
		select * 
		from dbo.sesScheduleEvents
		where UserId = @HostUserId
			and Start <= @Start
			and [End] >= @End
			and @Start > @Now
			and [Type] = 'SES_VT'
	)
		raiserror('%s,%d:: The user is not available at that time.', 16, 1, @ProcName, @HostUserId);

	if exists (
		select * 
		from dbo.sesScheduleEvents
		where UserId = @GuestUserId
			and Start < @End
			and [End] > @Start
			and dbo.sesIsSession([Type]) = 1
	)
		raiserror('%s,%d:: The user has another session at that time.', 16, 1, @ProcName, @GuestUserId);

	-- Check the price.
	if not exists (
		select *
			from dbo.appUsers U
				left join dbo.friFriends F on U.Id = F.UserId and F.FriendUserId = @GuestUserId
			where U.Id = @HostUserId
				and cast((coalesce(F.SessionRate, U.SessionRate, 0) * datediff(minute, @Start, @End) / 60.0) as decimal(9,2)) = @Price
	) begin
		declare @PriceText nvarchar(100) = cast(@Price as nvarchar(100));
		raiserror('%s,%d:: The price is wrong.', 16, 1, @ProcName, @PriceText);
	end

	-- The guest must have the Skype name entered in the profile
	if exists (
		select *
		from dbo.appUsers
		where Id = @GuestUserId
			and nullif(SkypeName, '') is null
	)
		raiserror('%s,%d:: Please enter your Skype name on the Profile page.', 16, 1, @ProcName, @GuestUserId);

	if @ExternalTran = 0
		begin transaction;

			-- Remove the vacant time slots from the schedules of both the users.
			exec dbo.sesDeleteVacantTime @UserId = @HostUserId, @Start = @Start, @End = @End;

			exec dbo.sesDeleteVacantTime @UserId = @GuestUserId, @Start = @Start, @End = @End, @VacantTimeFound = @GuestVacantTimeFound output;
			
			insert dbo.sesSessions (HostUserId, GuestUserId, Start, [End], Price, RequestTime, GuestHasVacantTime)
				values (@HostUserId, @GuestUserId, @Start, @End, @Price, @Now, @GuestVacantTimeFound);

			select @SessionId = scope_identity() where @@rowcount != 0;

			if @SessionId is null
				raiserror('%s,%d,%d:: User failed to create session request.', 16, 1, @ProcName, @HostUserId, @GuestUserId);

			-- One session produces two schedule items, one item per user. 
			insert dbo.sesScheduleEvents (UserId, Start, [End], [Type], Attribute)
				values (@HostUserId, @Start, @End, 'SESSRQ', @SessionId);

			insert dbo.sesScheduleEvents (UserId, Start, [End], [Type], Attribute)
				values (@GuestUserId, @Start, @End, 'SESSRQ', @SessionId);

			set @Attribute = cast(@SessionId as nvarchar(100));

			if @Price > 0
				exec dbo.accChangeEscrow @GuestUserId, @Price, 'TRSSRQ', @Attribute, null;

			exec dbo.appPostMessage	
				@SenderUserId = @GuestUserId, @RecipientUserId = @HostUserId,	@Type = 'MSSSRQ', @Attribute = @Attribute, @ExtId = @MessageExtId;

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
    ON OBJECT::[dbo].[sesRequestSession] TO [websiterole]
    AS [dbo];

