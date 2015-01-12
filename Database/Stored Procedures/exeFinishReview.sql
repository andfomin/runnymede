


CREATE PROCEDURE [dbo].[exeFinishReview]
	@ReviewId int,
	@UserId int
AS
BEGIN
/*
20121117 AF.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Price decimal(9,2);

	select @Price = Price
	from dbo.exeReviews 
	where Id = @ReviewId
		and UserId = @UserId 
		and StartTime is not null
		and FinishTime is null;

	-- Price is not nullable.
	if @Price is null
		raiserror('%s,%d:: The review cannot be finished.', 16, 1, @ProcName, @ReviewId);

	if (@Price > 0) begin

		exec dbo.accFinishReview @ReviewId;

	end
	else begin

		if @ExternalTran = 0
			begin transaction;

			update dbo.exeReviews
				set FinishTime = sysutcdatetime()
			where Id = @ReviewId 
				and UserId = @UserId 
				and StartTime is not null
				and FinishTime is null;

			if @@rowcount = 0
				raiserror('%s,%d,%d:: The user failed to finish the review.', 16, 1, @ProcName, @UserId, @ReviewId);

		if @ExternalTran = 0
			commit;
	
	end

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
    ON OBJECT::[dbo].[exeFinishReview] TO [websiterole]
    AS [dbo];

