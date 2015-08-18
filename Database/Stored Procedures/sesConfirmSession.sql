

CREATE PROCEDURE [dbo].[sesConfirmSession]
	@UserId int,
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SkypeName nvarchar(100), @IsTeacher bit;

	select @IsTeacher = IsTeacher, @SkypeName = SkypeName
	from dbo.appUsers 
	where Id = @UserId;

	if (nullif(@IsTeacher, 0) is null)
		raiserror('%s,%d,%d:: The user is not a teacher.', 16, 1, @ProcName, @UserId, @SessionId);

	if (nullif(@SkypeName, '') is null)
		raiserror('%s,%d:: Please enter your Skype name on the Profile page.', 16, 1, @ProcName, @UserId);

	if exists (
		select * 
		from dbo.sesSessions S1,
			(
				select Start, [End] from dbo.sesSessions where Id = @SessionId
			) S2
		where S1.Start < S2.[End]
			and S1.[End] > S2.Start
			and TeacherUserId = @UserId
			and ConfirmationTime is not null
			and CancellationTime is null 
	)
		raiserror('%s,%d:: You have another session at the time.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;
			
			update dbo.sesSessions
				set ConfirmationTime = sysutcdatetime()
			output inserted.ConfirmationTime
			where Id = @SessionId
				and TeacherUserId = @UserId
				and Start > sysutcdatetime()
				and ConfirmationTime is null
				and CancellationTime is null
				and ClosingTime is null
				;

			if @@rowcount = 0
				raiserror('%s,%d:: Failed to confirm the session.', 16, 1, @ProcName, @UserId);

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

