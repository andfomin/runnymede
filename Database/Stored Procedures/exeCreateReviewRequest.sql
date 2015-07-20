


CREATE PROCEDURE [dbo].[exeCreateReviewRequest]
	@UserId int,
	@ExerciseId int
AS
BEGIN
/*
Publishes exercise for reviewing. 

20121113 AF. Initial release
20150313 AF. Single price. Any reviewer.
20150325 AF. Do not use escrow. Post revenue directly at this moment.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Price decimal(9,2), @ServiceType1 char(6), @Attribute nvarchar(100), @ReviewId int, @Now datetime2(2);

	-- Only the owner of the exercise can request a review. 
	select @Price = dbo.appGetServicePrice(ServiceType)
	from dbo.exeExercises 
	where Id = @ExerciseId 
		and UserId = @UserId;

	if (@Price is null)
		raiserror('%s,%d,%d:: The user cannot request a review of the exercise.', 16, 1, @ProcName, @UserId, @ExerciseId);

	select @ReviewId = dbo.exeGetNewReviewId();

	set @Attribute = cast(@ReviewId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;

		exec dbo.accChangeEscrow @UserId = @UserId, @Amount = @Price, @TransactionType = 'TRRVRQ', @Attribute = @Attribute, @Details = null, @Now = @Now output;

		insert dbo.exeReviews (Id, ExerciseId, RequestTime)
			values (@ReviewId, @ExerciseId, @Now);		

	if @ExternalTran = 0
		commit;

	select @ReviewId as Id, @ExerciseId as ExerciseId, @Now as RequestTime;

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
    ON OBJECT::[dbo].[exeCreateReviewRequest] TO [websiterole]
    AS [dbo];

