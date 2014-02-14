
CREATE PROCEDURE [dbo].[relJoinSkypeDirectory]
	@UserId int,
	@Skype nvarchar(100),
	@Announcement nvarchar(200) = null
AS
BEGIN
/*
20140212 AF.
*/
SET NOCOUNT ON;
declare @ExternalTran int, @XState int, @ProcName sysname;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ProfileSkype nvarchar(100);

	select @ProfileSkype = Skype
	from dbo.appUsers 
	where Id = @UserId;

	if coalesce(@ProfileSkype, @Skype) ! = @Skype
	begin
		raiserror('%s,%d,%s,%s:: Skype name in the profile is different.', 16, 1, @ProcName, @UserId, @Skype, @ProfileSkype);  
	end;

	declare @Now datetime2(7) = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		insert dbo.relSkypeDirectory (UserId, Skype, TimeBegin, TimeEnd, Announcement)
			select @UserId, @Skype, @Now, null, @Announcement
			-- Prevent duplicate listings.
			where not exists (
				select * from dbo.relSkypeDirectory 
				where UserId = @UserId
				and @Now between TimeBegin and coalesce(TimeEnd, dateadd(day, 1, @Now))
			);

		if @@rowcount = 0
			raiserror('%s,%d:: User failed to join the Skype list.', 16, 1, @ProcName, @UserId);

		-- Use this opportunity to store the user's Skype name in the profile.
		if @ProfileSkype is null 
		begin 
			update dbo.appUsers 
			set Skype = @Skype
			where Id = @UserId;
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
    ON OBJECT::[dbo].[relJoinSkypeDirectory] TO [websiterole]
    AS [dbo];

