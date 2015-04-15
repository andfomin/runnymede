

CREATE PROCEDURE [dbo].[sesBookSession]
	@UserId int, -- who requests
	@Start smalldatetime,
	@End smalldatetime,
	@Price decimal(9,2),
	@TeacherUserId int
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

	declare @t table (
		SessionId int
	);

	declare @Attribute nvarchar(100);

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

	if not exists (
		select *
		from dbo.sesGetSessionPrice(@Start, @End)
		where Price = @Price
	)
		raiserror('%s,%d:: The price is wrong', 16, 1, @ProcName, @UserId);

	if exists (
		select * 
		from dbo.sesSessions
		where Start < @End
			and [End] > @Start
			and LearnerUserId = @UserId
	)
		raiserror('%s,%d:: The user has another session at the time.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;
			
			update dbo.sesSessions
				set LearnerUserId = @UserId, BookingTime = sysutcdatetime()
			output inserted.Id
				into @t 
			where Id = (
				select top (1) Id
				from dbo.sesSessions S
				where S.Start = @Start
					and S.[End] = @End
					and S.Price = @Price
					and nullif(@TeacherUserId, S.TeacherUserId) is null			
			);

			if @@rowcount = 0
				raiserror('%s,%d:: Session not found.', 16, 1, @ProcName, @UserId);

			select @Attribute = cast(SessionId as nvarchar(100)) from @t;

			exec dbo.accChangeEscrow @UserId, @Price, 'TRSSRQ', @Attribute, null;

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
    ON OBJECT::[dbo].[sesBookSession] TO [websiterole]
    AS [dbo];

