

CREATE PROCEDURE [dbo].[sesCloseSession]
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @FinishTime datetime2(2);

	if @ExternalTran = 0
		begin transaction;

		exec dbo.accFinishSession @SessionId, @FinishTime = @FinishTime output;

		update dbo.sesSessions			 
			set ClosingTime = @FinishTime
		where Id = @SessionId
			and ClosingTime is null
			and (
				-- Give the learner an opportunity to dispute the session before it is closed programmatically.
				datediff(minute, [End], @FinishTime) >= dbo.appGetConstantAsInt('Sessions.ClosingDelay.Minutes') 
				or Rating is not null				
			);

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to finish session.', 16, 1, @ProcName, @SessionId);


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
    ON OBJECT::[dbo].[sesCloseSession] TO [websiterole]
    AS [dbo];

