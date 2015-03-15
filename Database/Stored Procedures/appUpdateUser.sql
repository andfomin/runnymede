

CREATE PROCEDURE [dbo].[appUpdateUser]
-- If a parameter is null, that meens keep the corresponding field intact.
	@UserId int,
	@DisplayName nvarchar(100) = null,
	@SkypeName nvarchar(100) = null,
	@Announcement nvarchar(1000) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	-- Coalesce() uses the data type with highest precedence as the data type of the result. Throws Error converting data type nvarchar to numeric. 
	if (
		(@DisplayName is null) and
		(@SkypeName is null) and
		(@Announcement is null)
	)
			raiserror('%s,%d:: Cannot update the user profile.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.appUsers set 
			DisplayName = coalesce(@DisplayName, DisplayName),
			SkypeName = coalesce(@SkypeName, SkypeName),
			Announcement = coalesce(@Announcement, Announcement)
		where Id = @UserId;

		if @@rowcount = 0
			raiserror('%s,%d:: The user profile update failed.', 16, 1, @ProcName, @UserId);

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

