


CREATE PROCEDURE [dbo].[exeDeleteSuggestion]
	@ReviewId int,
	@CreationTime int,
	@UserId int
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

		delete S
			from dbo.exeSuggestions S
				inner join dbo.exeReviews R on S.ReviewId = R.Id
			where S.ReviewId = @ReviewId
				and S.CreationTime = @CreationTime
				and R.UserId = @UserId;
				
		--if (@@rowcount = 0) begin
		--	declare @CreationTimeText nvarchar(100);
		--	set @CreationTimeText = cast(@CreationTime as nvarchar(100)); 
		--	raiserror('%s,%d,%s,%d:: The user failed to delete the suggestion from the review.', 16, 1, @ProcName, @UserId, @CreationTimeText, @ReviewId);
		--end		

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
    ON OBJECT::[dbo].[exeDeleteSuggestion] TO [websiterole]
    AS [dbo];

