

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
	declare @ClaimType nvarchar(100) = dbo.appGetConstant('Users.ClaimTypes.IsTeacher');

	if @UserId is null or @ClaimType is null
		raiserror('%s,%s:: User or constant not found.', 16, 1, @ProcName, @UserName);

	if @ExternalTran = 0
		begin transaction;

		update dbo.appUsers set IsTeacher = 1 where Id = @UserId;

		merge dbo.aspnetUserClaims as target
			using (select @ClaimType, @UserId) as source (ClaimType, UserId)
			on (target.ClaimType = source.ClaimType and target.UserId = source.UserId)
			when matched then 
				update set ClaimValue = '' -- the value does not matter, the presense of the claim does matter. A NULL value causes exception in ASP.NET Identity code.
			when not matched then	
				insert (ClaimType, ClaimValue, UserId)
				values (source.ClaimType, '', source.UserId);

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