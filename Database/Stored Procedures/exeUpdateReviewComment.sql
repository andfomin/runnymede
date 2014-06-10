

CREATE PROCEDURE [dbo].[exeUpdateReviewComment]
	@ReviewId int,
	@Comment nvarchar(4000),
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

        update dbo.exeReviews set Comment = @Comment 
		where Id = @ReviewId and UserId = @UserId
			and FinishTime is null;

		if (@@rowcount = 0)
			raiserror('%s,%d,%d::The user could not update the comment of the review.', 16, 1, @ProcName, @UserId, @ReviewId);			

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
    ON OBJECT::[dbo].[exeUpdateReviewComment] TO [websiterole]
    AS [dbo];

