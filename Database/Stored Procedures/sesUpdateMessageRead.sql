

CREATE PROCEDURE [dbo].[sesUpdateMessageRead]
	@MessageId int,
	@MessageExtId nchar(12) = null,
	@UserId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;
--raiserror('%s,%d::', 16, 1, @ProcName, @);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		update dbo.appMessages
			set ReceiveTime = sysutcdatetime()
		where Id = @MessageId
			and ExtId = @MessageExtId
			and RecipientUserId = @UserId
			and ReceiveTime is null;

		if @@rowcount = 0 
			raiserror('%s,%d,%d,%s:: Message update failed.', 16, 1, @ProcName, @UserId, @MessageId, @MessageExtId);

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
    ON OBJECT::[dbo].[sesUpdateMessageRead] TO [websiterole]
    AS [dbo];

