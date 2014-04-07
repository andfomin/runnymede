
CREATE PROCEDURE [dbo].[relPostSessionMessage]
	@EventId int,
	@UserId int,
	@Message nvarchar(1000) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname;
select  @ProcName = object_name(@@procid);

begin try

	declare @RecipientUserId int, @Attribute nvarchar(100), @Text nvarchar(1000);
	
	-- The sign of @EscrowAmount determines the direction of the transfer.
	select @RecipientUserId = case when UserId = @UserId then SecondUserId else UserId end
	from dbo.relScheduleEvents
	where Id = @EventId
		and (UserId = @UserId or SecondUserId = @UserId);

	if (@RecipientUserId is null)
		raiserror('%s,%d,%d:: The user is not related to the session.', 16, 1, @ProcName, @UserId, @EventId);

	set @Attribute = cast(@EventId as nvarchar(100));
			
	exec dbo.relPostMessage	
		@SenderUserId = @UserId, @RecipientUserId = @RecipientUserId, @Type = 'SSSN', @Attribute = @Attribute, @Text = @Message;

end try
begin catch
	throw;
end catch

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[relPostSessionMessage] TO [websiterole]
    AS [dbo];

