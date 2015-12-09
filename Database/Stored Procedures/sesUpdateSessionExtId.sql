

CREATE PROCEDURE [dbo].[sesUpdateSessionExtId]
	@SessionId int,
	@ExtId bigint = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		update dbo.sesSessions set ExtId = @ExtId 
		where Id = @SessionId;
		
		if @@rowcount = 0
			raiserror('%s,%d:: Failed to update the session.', 16, 1, @ProcName, @SessionId);

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
    ON OBJECT::[dbo].[sesUpdateSessionExtId] TO [websiterole]
    AS [dbo];

