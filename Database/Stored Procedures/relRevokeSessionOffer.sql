


CREATE PROCEDURE [dbo].[relRevokeSessionOffer]
	@UserId int,
	@Id int
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

		update dbo.relScheduleEvents
		set [Type] = 'ROFR', CancellationTime = sysutcdatetime()
		where Id = @Id
			and UserId = @UserId
			and [Type] = 'OFFR'
			and CancellationTime is null
			and [End] > sysutcdatetime();

		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to revoke session offer.', 16, 1, @ProcName, @UserId, @Id);

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
    ON OBJECT::[dbo].[relRevokeSessionOffer] TO [websiterole]
    AS [dbo];

