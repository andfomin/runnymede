

CREATE PROCEDURE [dbo].[sesBookSession]
	@UserId int, -- who requests
	@Start smalldatetime,
	@End smalldatetime,
	@TeacherUserId int,
	@Price decimal(9,2),
	@MessageExtId nchar(12) = null
AS
BEGIN
/*
The host may be indicated explicitly. If @TeacherUserId is null, it means the user chose any host which he has had no contact with.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SessionId int, @Attribute nvarchar(100), @Now datetime2(2);

	declare @Advance int = dbo.appGetConstantAsInt('Sessions.BookingAdvance.Minutes');

	if (@Start < dateadd(minute, @Advance, sysutcdatetime()))
		raiserror('%s,%d:: Book a session at least %d minutes in advance', 16, 1, @ProcName, @UserId, @Advance);

	-- The user must have the Skype name entered in the profile
	if exists (
		select *
		from dbo.appUsers
		where Id = @UserId
			and nullif(SkypeName, '') is null
	)
		raiserror('%s,%d:: Please enter your Skype name on the Profile page.', 16, 1, @ProcName, @UserId);

    if (@Price != (dbo.appGetServicePrice('SVSSSN') * datediff(minute, @Start, @End) / 60))
		raiserror('%s,%d:: The price is wrong', 16, 1, @ProcName, @UserId);

	if not exists (
		select *
		from dbo.appUsers 
		where Id = @TeacherUserId
			and IsTeacher = 1
	)
		raiserror('%s,%d,%d:: The user is not a teacher.', 16, 1, @ProcName, @UserId, @TeacherUserId);

	if exists (
		select * 
		from dbo.sesSessions
		where Start < @End
			and [End] > @Start
			and LearnerUserId = @UserId
			and CancellationTime is null
	)
		raiserror('%s,%d:: The user has another session at the time.', 16, 1, @ProcName, @UserId);

	select @SessionId = dbo.sesGetNewSessionId();
	
	select @Attribute = cast(@SessionId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;
			
		-- The service may be offered to the user for free.
		update dbo.appGiveaways
		set [Counter] = [Counter] - 1
		where UserId = @UserId
			and ServiceType = 'SVSSSN'
			and [Counter] > 0;

		if (@@rowcount != 0)
			set @Price = 0;

		exec dbo.accChangeEscrow 
			@UserId = @UserId, @Amount = @Price, @Increase = 1, @TransactionType = 'TRSSRQ', @Attribute = @Attribute, @Details = null, @Now = @Now output;

		insert dbo.sesSessions (Id, Start, [End], LearnerUserId, TeacherUserId, Price, BookingTime)			
		values (@SessionId, @Start, @End, @UserId, @TeacherUserId, @Price, @Now);

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to book the session.', 16, 1, @ProcName, @UserId);

		if (@MessageExtId is not null) begin
			exec dbo.appPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @TeacherUserId, @Type = 'MSSSRQ', @Attribute = @Attribute, @ExtId = @MessageExtId;
		end

	if @ExternalTran = 0
		commit;

	select @SessionId;

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
    ON OBJECT::[dbo].[sesBookSession] TO [websiterole]
    AS [dbo];

