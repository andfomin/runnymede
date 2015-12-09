


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
20150721 AF. Use escrow.
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Price decimal(9,2), @ServiceType char(6), @Attribute nvarchar(100), @ReviewId int, @Now datetime2(2), @TeacherUserId int;

	-- Only the owner of the exercise can request a review. 
	select @Price = dbo.appGetServicePrice(ServiceType), @ServiceType = ServiceType
	from dbo.exeExercises 
	where Id = @ExerciseId 
		and UserId = @UserId;

	if (@Price is null)
		raiserror('%s,%d,%d:: The user cannot request a review of the exercise.', 16, 1, @ProcName, @UserId, @ExerciseId);

	set @Now = sysutcdatetime();

	-- If there is an ongoing session, make the review started. The price for such a review is 0.
	select @TeacherUserId = TeacherUserId
	from dbo.sesSessions
	where Start <= @Now
		and [End] > @Now
		and LearnerUserId = @UserId
		and ConfirmationTime is not null
		and CancellationTime is null;
	
	select @ReviewId = dbo.exeGetNewReviewId();

	set @Attribute = cast(@ReviewId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;

		-- The service may be offered to the user for free.
		update dbo.appGiveaways
		set [Counter] = [Counter] - 1
		where UserId = @UserId
			and ServiceType = @ServiceType
			and [Counter] > 0;

		if ((@@rowcount != 0) or (@TeacherUserId is not null))
			set @Price = 0;

		exec dbo.accChangeEscrow
			@UserId = @UserId, @Amount = @Price, @Increase = 1, @TransactionType = 'TRRVRQ', @Attribute = @Attribute, @Details = null, @Now = @Now output;

		insert dbo.exeReviews (Id, ExerciseId, RequestTime, UserId, StartTime)
			values (@ReviewId, @ExerciseId, @Now, @TeacherUserId, iif(@TeacherUserId is null, null, @Now));		

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

