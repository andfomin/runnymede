

CREATE PROCEDURE [dbo].[appPostMessage]
	@SenderUserId int,
	@RecipientUserId int,
	@Type char(6),
	@Attribute nvarchar(100) = null,
	@ExtId nchar(12) = null
AS
BEGIN
/*
We save only short snippet of a message in the SQL database. 
If the message length exceds the limit, the web server saves the full text in Storage Table and passes here the ExtId.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @SenderDisplayName nvarchar(100), @RecipientDisplayName nvarchar(100);
	
	select @SenderDisplayName = DisplayName 
	from dbo.appUsers 
	where Id = @SenderUserId;

	select @RecipientDisplayName = DisplayName 
	from dbo.appUsers 
	where Id = @RecipientUserId;

	if ((@SenderDisplayName is null) or (@RecipientDisplayName is null))
		raiserror('%s,%d,%d:: User not found.', 16, 1, @ProcName, @SenderUserId, @RecipientUserId);

	if @ExternalTran = 0
		begin transaction;

		insert dbo.appMessages ([Type], PostTime, 
				SenderUserId, SenderDisplayName, 
				RecipientUserId, RecipientDisplayName, 
				Attribute, ExtId)
			values (@Type, sysutcdatetime(), 
				@SenderUserId, @SenderDisplayName, 
				@RecipientUserId, @RecipientDisplayName, 
				@Attribute, @ExtId);

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