


CREATE PROCEDURE [dbo].[exeCreateReviewRequest]
	@UserId int,
	@ExerciseId int,
	@Price decimal(9,2)
AS
BEGIN
/*
Publishes exercise for reviewing. 

20121113 AF. Initial release

20150313 AF. Single price. Any reviewer.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ExerciseType char(6), @ExerciseLength int,	@ReviewId int, @Attribute nvarchar(100), @Now datetime2(2);

	-- Only the owner of the exercise can request a review. 
	select @ExerciseType = [Type], @ExerciseLength = [Length] 
	from dbo.exeExercises 
	where Id = @ExerciseId 
		and UserId = @UserId;
	
	-- Type is non-nullable.
	if (@ExerciseType is null)
		raiserror('%s,%d,%d:: The user cannot request a review of the exercise.', 16, 1, @ProcName, @UserId, @ExerciseId);

   if (@Price <> dbo.exeCalculateReviewPrice(@ExerciseType, @ExerciseLength))
		raiserror('%s,%d,%d:: The price is wrong.', 16, 1, @ProcName, @UserId, @ExerciseId);

	select @ReviewId = dbo.exeGetNewReviewId();

	set @Attribute = cast(@ReviewId as nvarchar(100));

	set @Now = sysutcdatetime();

	if @ExternalTran = 0
		begin transaction;

		insert dbo.exeReviews (Id, ExerciseId, Price, RequestTime)
			values (@ReviewId, @ExerciseId, @Price, @Now);		

		exec dbo.accChangeEscrow @UserId = @UserId, @Amount = @Price, @TransactionType = 'TRRVRQ', @Attribute = @Attribute;

	if @ExternalTran = 0
		commit;

	select @ReviewId as Id, @ExerciseId as ExerciseId, @Price as Price, @Now as RequestTime;

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

