
CREATE PROCEDURE [dbo].[libDeletePersonalResource]
	@UserId int,
	@ResourceId int
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

		update dbo.libUserResources
			set IsPersonal = 0, Comment = null, ReindexSearch = 1
		where UserId = @UserId
			and ResourceId = @ResourceId;

		if (@@rowcount = 0) 
			raiserror('%s,%d,%d:: Failed to remove personal resource.', 16, 1, @ProcName, @UserId, @ResourceId);

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
    ON OBJECT::[dbo].[libDeletePersonalResource] TO [websiterole]
    AS [dbo];

