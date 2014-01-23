

CREATE PROCEDURE [dbo].[appUpdateUser]
-- If a parameter is null, that meens keep the corresponding field intact.
	@UserId int,
	@DisplayName nvarchar(100) = null,
	@Skype nvarchar(200) = null,
	@RateARec decimal(18,2) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if (coalesce(@DisplayName, @Skype, @RateARec) is null)
			raiserror('%s,%d:: All parameters are null. Cannot update the profile for the user.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.appUsers set 
			[DisplayName] = coalesce(@DisplayName, DisplayName),
			Skype = coalesce(@Skype, Skype),
			RateARec = coalesce(@RateARec, RateARec)
		where Id = @UserId;

		if @@rowcount = 0
			raiserror('%s,%d:: The user profile update failed.', 16, 1, @ProcName, @UserId);

		if (@DisplayName is not null) begin
			update dbo.aspnetUserClaims set
				ClaimValue = @DisplayName
			where UserId = @UserId
				and ClaimType = 'englc.com/DisplayName';

			if @@rowcount = 0
				raiserror('%s,%d:: Name claim update failed.', 16, 1, @ProcName, @UserId);
		end	

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
    ON OBJECT::[dbo].[appUpdateUser] TO [websiterole]
    AS [dbo];

