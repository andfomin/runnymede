
CREATE PROCEDURE [dbo].[sysInitializeUser]
	@UserName nvarchar(128), -- = N's@s.s',
	@PasswordHash nvarchar(128), -- = N'AMple7VrgoAu2SmGdF5bboa+56qatcNYFJ/KGFfpTWQOhkJ4ow0fe/ZfIh0EIR5qlQ==', --'123456'
	@SecurityStamp nvarchar(max),
	@DisplayName nvarchar(100), -- = N'sss'
	@IsTeacher bit
AS
BEGIN
	SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

	declare @UserId int;

	select @UserId = dbo.appGetNewUserId();

	insert dbo.aspnetUsers (Id, UserName, PasswordHash, SecurityStamp, Email) 
		values (@UserId, @UserName, @PasswordHash, @SecurityStamp, @UserName);

	insert dbo.appUsers (Id, DisplayName, IsTeacher) values (@UserId, @DisplayName, iif(@IsTeacher = 1, 1, null));

	-- We create user accounts as needed, on the fly.

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