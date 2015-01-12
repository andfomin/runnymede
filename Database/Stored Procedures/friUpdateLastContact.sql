


CREATE PROCEDURE [dbo].[friUpdateLastContact]
	@UserId int,
	@FriendUserId int,	
	@ContactType nchar(2)
AS
BEGIN
/* 
It is called internally. Do not set access permitions.
*/
SET NOCOUNT ON;

if (@UserId != @FriendUserId) begin

	declare @ProcName sysname, @ExternalTran int, @XState int;
	select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

	begin try

		if @ExternalTran > 0
			save transaction ProcedureSave;

		if @ExternalTran = 0
			begin transaction;

			update dbo.friFriends
			set LastContactType = @ContactType, LastContactDate = sysutcdatetime()
			where UserId = @UserId and FriendUserId = @FriendUserId;

			if @@rowcount = 0 begin

				insert dbo.friFriends (UserId, FriendUserId, IsActive, LastContactType, LastContactDate)
				values (@UserId, @FriendUserId, 1, @ContactType, sysutcdatetime());

				-- Friendship must be symmetrical.
				-- This also prevents a user to become a friend of themselves. The duplicate key value is (X, X).
				insert dbo.friFriends (UserId, FriendUserId, IsActive)
					values (@FriendUserId, @UserId, 1);

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

end

END