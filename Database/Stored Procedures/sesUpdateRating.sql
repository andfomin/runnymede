


CREATE PROCEDURE [dbo].[sesUpdateRating]
	@UserId int,
	@SessionId int,
	@Rating tinyint
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @t table (ClosingTime smalldatetime);

	if @ExternalTran = 0
		begin transaction;

		update dbo.sesSessions
			set Rating = @Rating
		output inserted.ClosingTime into @t
		where Id = @SessionId
			and LearnerUserId = @UserId
			and Start < sysutcdatetime();

		if @@rowcount = 0 
			raiserror('%s,%d,%d:: Session update failed.', 16, 1, @ProcName, @UserId, @SessionId);

		if exists (select * from @t where ClosingTime is null) begin

			exec dbo.sesCloseSession @SessionId;

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
    ON OBJECT::[dbo].[sesUpdateRating] TO [websiterole]
    AS [dbo];

