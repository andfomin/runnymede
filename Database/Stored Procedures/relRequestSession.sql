

CREATE PROCEDURE [dbo].[relRequestSession]
	@UserId int, -- teacher
	@SecondUserId int, -- learner
	@Start smalldatetime,
	@End smalldatetime,
	@Message nvarchar(1000) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @EventId int, @Attribute nvarchar(100), @Text nvarchar(1000), @Price decimal(18,2);

	set @Text = 'SESSION REQUESTED. ' + coalesce(@Message, '');

	--if not exists (select * from dbo.appUsers where Id = @UserId and IsTeacher = 1)
	--	raiserror('%s,%d:: The user is not a teacher.', 16, 1, @ProcName, @UserId);

	if not exists (
		select * 
		from dbo.relScheduleEvents
		where UserId = @UserId
			and Start <= @Start
			and [End] >= @End
			and [Type] = 'OFFR'
	)
		raiserror('%s,%d:: The user is not available at that time.', 16, 1, @ProcName, @UserId);

	if exists (
		select *
		from dbo.relScheduleEvents S
		where UserId = @UserId
			and Start < @End
			and [End] > @Start
			and [Type] in ('RQSN', 'CFSN')
	)
		raiserror('%s,%d:: The user has already got another session at the time.', 16, 1, @ProcName, @UserId);

	-- We allow @Price to be null to make possible sessions between two learners.
	select @Price = 
		case when IsTeacher = 1 
		then SessionRate * datediff(minute, @Start, @End) / 60.0
		else null end 
	from dbo.appUsers 
	where Id = @UserId;	

	if @ExternalTran = 0
		begin transaction;

			insert dbo.relScheduleEvents (UserId, Start, [End], [Type], SecondUserId, Price, CreationTime)
				select @UserId, @Start, @End, 'RQSN', @SecondUserId, @Price, sysutcdatetime();

			select @EventId = scope_identity() where @@rowcount != 0;

			if @EventId is null
				raiserror('%s,%d,%d:: User failed to create session request.', 16, 1, @ProcName, @SecondUserId, @UserId);
			
			set @Attribute = cast(@EventId as nvarchar(100));

			if @Price is not null
				exec dbo.accChangeEscrow @SecondUserId, @Price, 'SNRQ', @Attribute, null;

			exec dbo.relPostMessage	
				@SenderUserId = @SecondUserId, @RecipientUserId = @UserId,	@Type = 'SSSN', @Attribute = @Attribute, @Text = @Text;

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
    ON OBJECT::[dbo].[relRequestSession] TO [websiterole]
    AS [dbo];

