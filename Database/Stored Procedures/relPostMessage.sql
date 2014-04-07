

CREATE PROCEDURE [dbo].[relPostMessage]
	@SenderUserId int,
	@RecipientUserId int,
	@Type nchar(4),
	@Attribute nvarchar(100) = null,
	@Text nvarchar(1000) = null
AS
BEGIN
/*
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SenderDisplayName nvarchar(100), @RecepientDisplayName nvarchar(100);
	
	select @SenderDisplayName = DisplayName from dbo.appUsers where Id = @SenderUserId;

	select @RecepientDisplayName = DisplayName from dbo.appUsers where Id = @RecipientUserId;

	if ((@SenderDisplayName is null) or (@RecepientDisplayName is null))
		raiserror('%s,%d,%d:: User not found.', 16, 1, @ProcName, @SenderUserId, @RecipientUserId);

	if @ExternalTran = 0
		begin transaction;

		insert dbo.relMessages ([Type], PostTime, SenderUserId, RecipientUserId, SenderDisplayName, RecepientDisplayName, Attribute, [Text])
			values (@Type, sysutcdatetime(), @SenderUserId, @RecipientUserId, @SenderDisplayName, @RecepientDisplayName, @Attribute, @Text);

		if @@rowcount = 0
			raiserror('%s,%d,%s:: User failed to post message.', 16, 1, @ProcName, @SenderUserId, @Type);

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