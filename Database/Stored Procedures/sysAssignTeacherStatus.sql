

CREATE PROCEDURE [dbo].[sysAssignTeacherStatus]
	@UserName nvarchar(200)
AS
BEGIN
/*
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @UserId int = dbo.appGetUserId(@UserName);

	if @UserId is null
		raiserror('%s,%s:: User not found.', 16, 1, @ProcName, @UserName);

	if @ExternalTran = 0
		begin transaction;

		update dbo.appUsers set IsTeacher = 1 where Id = @UserId;

		exec dbo.accCreateAccountsIfNotExist @UserId = @UserId, @Types = N'<Types><Type>ACUCSH</Type></Types>';

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