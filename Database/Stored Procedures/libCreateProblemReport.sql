
CREATE PROCEDURE [dbo].[libCreateProblemReport]
	@UserId int,
	@ResourceId int,
	@Report nvarchar(4000)
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

		-- Insert new resource.
		insert dbo.libProblemReports (UserId, ResourceId, Report)
			values (@UserId, @ResourceId, @Report)

		select convert(int, @@identity) as Id where @@rowcount != 0;

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
    ON OBJECT::[dbo].[libCreateProblemReport] TO [websiterole]
    AS [dbo];

