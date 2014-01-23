
CREATE PROCEDURE [dbo].[_Template]
AS
BEGIN
/*
+http://rusanu.com/2010/11/22/try-catch-throw-exception-handling-in-t-sql/
+http://msdn.microsoft.com/en-us/library/ms188378.aspx
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;
--raiserror('%s,%d: ', 16, 1, @ProcName, @);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	-- 

	if @ExternalTran = 0
		begin transaction;

	--

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