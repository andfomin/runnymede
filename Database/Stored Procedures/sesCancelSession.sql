

CREATE PROCEDURE [dbo].[sesCancelSession]
	@UserId int,
	@SessionId int,
	@ClearUsers bit = null -- During the booking process, if we failed to book a corresponding external session, make the session abandoned.
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Price decimal(9,2), @LearnerUserId int, @Attribute nvarchar(100), @Now datetime2(2);

	select @Price = Price, @LearnerUserId = LearnerUserId
	from dbo.sesSessions
	where Id = @SessionId
		and TeacherUserId = iif(@ClearUsers = 1, TeacherUserId, @UserId)
		and Start > sysutcdatetime()
		and ConfirmationTime is null
		and CancellationTime is null
		and ClosingTime is null
	;

	if (@LearnerUserId is null)
		raiserror('%s,%d,%d:: Cannot cancel the session', 16, 1, @ProcName, @UserId, @SessionId);

	select @Attribute = cast(@SessionId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;
			
			exec dbo.accChangeEscrow 
				@UserId = @LearnerUserId, @Amount = @Price, @Increase = 0, @TransactionType = 'TRSSCN', @Attribute = @Attribute, @Details = null, @Now = @Now output;

			update dbo.sesSessions
				set CancellationTime = @Now, 
				CancellationUserId = iif(@ClearUsers = 1, null, @UserId), 
				LearnerUserId = iif(@ClearUsers = 1, null, LearnerUserId),
				TeacherUserId = iif(@ClearUsers = 1, null, TeacherUserId)
			output inserted.CancellationTime
			where Id = @SessionId
				and TeacherUserId = iif(@ClearUsers = 1, TeacherUserId, @UserId)
				and Start > @Now
				and ConfirmationTime is null
				and CancellationTime is null
				and ClosingTime is null
				;

			if @@rowcount = 0
				raiserror('%s,%d,%d:: Failed to cancel the session.', 16, 1, @ProcName, @SessionId, @UserId);
			

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

