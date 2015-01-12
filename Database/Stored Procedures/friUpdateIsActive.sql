

CREATE PROCEDURE [dbo].[friUpdateIsActive]
	@UserId int,
	@FriendUserId int,	
	@IsActive bit
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

		update dbo.friFriends 	
		set IsActive = @IsActive
		where UserId = @UserId
			and FriendUserId = @FriendUserId;

		if @@rowcount = 0
			raiserror('%s,%d::Friendship not found.', 16, 1, @ProcName, @UserId);

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
    ON OBJECT::[dbo].[friUpdateIsActive] TO [websiterole]
    AS [dbo];

