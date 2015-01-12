CREATE PROCEDURE [dbo].[exeCancelReviewRequest]
	@ReviewId int,
	@UserId int
AS
BEGIN
/*
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @EscrowAmount decimal(18, 2), @Dummy int, @Attribute nvarchar(100);

	-- Negative value passed to dbo.accChangeEscrow means return escrow.
	select @EscrowAmount = 0 - R.Price, @Dummy = R.Id
		from dbo.exeReviews R
			inner join dbo.exeExercises E on R.ExerciseId = E.Id
		where R.Id = @ReviewId 
			and E.UserId = @UserId
			and R.StartTime is null
			and R.CancelationTime is null;

	-- The review cannot be canceled
	if @Dummy is null
		raiserror('%s,%d,%d:: The user cannot cancel the review request.', 16, 1, @ProcName, @UserId, @ReviewId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.exeReviews 
		set CancelationTime = sysutcdatetime()
		where Id = @ReviewId 
			and StartTime is null 
			and CancelationTime is null;		
				
		if @@rowcount = 0
			raiserror('%s,%d,%d:: The user failed to cancel the review.', 16, 1, @ProcName, @UserId, @ReviewId);	
				
		update dbo.exeRequests
		set IsActive = 0
		where ReviewId = @ReviewId;

		set @Attribute = cast(@ReviewId as nvarchar(100));

		if @EscrowAmount != 0
			exec dbo.accChangeEscrow @UserId, @EscrowAmount, 'TRRVRC', @Attribute, null;

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
    ON OBJECT::[dbo].[exeCancelReviewRequest] TO [websiterole]
    AS [dbo];

