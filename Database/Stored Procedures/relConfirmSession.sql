



CREATE PROCEDURE [dbo].[relConfirmSession]
	@EventId int,
	@UserId int,
	@Message nvarchar(1000) = null
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SecondUserId int, @Attribute nvarchar(100), @Text nvarchar(1000);
	declare @t table (SecondUserId int);

	set @Attribute = cast(@EventId as nvarchar(100));

	set @Text = 'SESSION DISPUTED. ' + coalesce(@Message, '');

	if @ExternalTran = 0
		begin transaction;

			update dbo.relScheduleEvents 
			set [Type] = 'CFSN', ConfirmationTime = sysutcdatetime()
			output deleted.SecondUserId into @t
			where Id = @EventId
				and UserId = @UserId
				and [Type] = 'RQSN'
				and (ConfirmationTime is null)
				and [End] > sysutcdatetime();

			if @@rowcount = 0
				raiserror('%s,%d,%d:: User failed to confirm session request', 16, 1, @ProcName, @UserId, @EventId);

			select @SecondUserId = SecondUserId from @t;

			exec dbo.relPostMessage	
				@SenderUserId = @UserId, @RecipientUserId = @SecondUserId,	@Type = 'SSSN', @Attribute = @Attribute, @Text = 'SESSION CONFIRMED.';

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
    ON OBJECT::[dbo].[relConfirmSession] TO [websiterole]
    AS [dbo];

