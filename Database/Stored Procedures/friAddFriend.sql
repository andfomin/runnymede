


CREATE PROCEDURE [dbo].[friAddFriend]
	@UserId int,
	@Email nvarchar(200)
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try

	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @FriendUserId int;

	select @FriendUserId = Id
	from aspnetUsers
	where Email = @Email

	if @FriendUserId is null 
		raiserror('%s,%d,%s:: Friend not found', 16, 1, @ProcName, @UserId, @Email);

	if exists (
		select UserId 
		from dbo.friFriends 
		where UserId = @UserId and FriendUserId = @FriendUserId
		-- Friendship must be symmetrical. We may check one side of the friendship.
		--union 
		--select UserId 
		--from dbo.parPartners 
		--where UserId = @FriendUserId and FriendUserId = @UserId
	)
		raiserror('%s,%d,%s:: Friendship already exists.', 16, 1, @ProcName, @UserId, @Email);

	if @ExternalTran = 0
		begin transaction;

			-- PK is UserId + FriendUserId.
			insert dbo.friFriends (UserId, FriendUserId, IsActive, LastContactDate, LastContactType)
				values (@UserId, @FriendUserId, 1, sysutcdatetime(), 'CN__AE');

			-- Add a symmetrical relation.
			insert dbo.friFriends (UserId, FriendUserId, IsActive)
				values (@FriendUserId, @UserId, 1);

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
    ON OBJECT::[dbo].[friAddFriend] TO [websiterole]
    AS [dbo];

