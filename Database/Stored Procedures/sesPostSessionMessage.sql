

CREATE PROCEDURE [dbo].[sesPostSessionMessage]
	@SessionId int,
	@UserId int,
	@MessageExtId nchar(12) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @RecipientUserId int, @Attribute nvarchar(100);
	
	select @RecipientUserId = iif(HostUserId = @UserId, GuestUserId, HostUserId)
	from dbo.sesSessions
	where Id = @SessionId
		and (HostUserId = @UserId or GuestUserId = @UserId)
		and FinishTime is null;

	if (@RecipientUserId is null)
		raiserror('%s,%d,%d:: The user cannot send message for a finished session.', 16, 1, @ProcName, @UserId, @SessionId);

	set @Attribute = cast(@SessionId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;

		exec dbo.appPostMessage	
			@SenderUserId = @UserId, @RecipientUserId = @RecipientUserId, @Type = 'MSSSMS', @Attribute = @Attribute, @ExtId = @MessageExtId;

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
    ON OBJECT::[dbo].[sesPostSessionMessage] TO [websiterole]
    AS [dbo];

