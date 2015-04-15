

CREATE PROCEDURE [dbo].[libResetIndexedUserResources]
	@UserResources xml
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

		update UR set 
			ReindexSearch = 0			
		from dbo.libUserResources UR
			inner join @UserResources.nodes('/UserResources/UR') T(C) on UR.UserId = T.C.value('@U[1]', 'int') and UR.ResourceId = T.C.value('@R[1]', 'int')

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
    ON OBJECT::[dbo].[libResetIndexedUserResources] TO [websiterole]
    AS [dbo];

