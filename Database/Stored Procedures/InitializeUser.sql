
CREATE PROCEDURE [dbo].[InitializeUser]
	@UserName nvarchar(128), -- = N's@s.s',
	@PasswordHash nvarchar(128), -- = N'AMple7VrgoAu2SmGdF5bboa+56qatcNYFJ/KGFfpTWQOhkJ4ow0fe/ZfIh0EIR5qlQ==', --'123456'
	@SecurityStamp nvarchar(max),
	@DisplayName nvarchar(100), -- = N'sss'
	@IsTutor bit
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

insert dbo.aspnetUsers (UserName, PasswordHash, SecurityStamp, Email) values (@UserName, @PasswordHash, @SecurityStamp, @UserName);

select @UserId = Id from dbo.aspnetUsers where Id = scope_identity() and @@rowcount != 0;

exec dbo.appCreateUserAndAccounts @UserId = @UserId, @DisplayName = @DisplayName;

insert dbo.aspnetUserClaims (ClaimType, ClaimValue, UserId) values ('englc.com/DisplayName', @DisplayName, @UserId);

if (@IsTutor = 1) begin
	insert dbo.aspnetUserClaims (ClaimType, ClaimValue, UserId) values ('englc.com/IsTutor', 'true', @UserId);
	update dbo.appUsers set IsTutor = 1 where Id = @UserId;
end;

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