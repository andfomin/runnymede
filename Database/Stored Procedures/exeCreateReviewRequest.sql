


CREATE PROCEDURE [dbo].[exeCreateReviewRequest]
	@ExerciseId int,
	@AuthorUserId int,
	@Reward decimal(18, 2),
	@ReviewerUserIds dbo.appUsersType readonly
AS
BEGIN
/*
Publishes exercise for reviewing.

20121113 AF. Initial release
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @ReviewerCount int, @AuthorName nvarchar(100), @TypeId nchar(4), @Length int, @Attribute nvarchar(100);

	if (@Reward < 0) begin
		declare @RewardText nvarchar(100) = cast(@Reward as nvarchar(100));
		raiserror('%s,%d,%s:: The user has offered negative reward.', 16, 1, @ProcName, @AuthorUserId, @RewardText);
	end;

	-- Only the owner of the exercise can request a review. TypeId is non-nullable.
	select @TypeId = TypeId, @Length = [Length] from dbo.exeExercises where Id = @ExerciseId and UserId = @AuthorUserId
	if (@TypeId is null)
		raiserror('%s,%d,%d:: The user cannot request review for the exercise.', 16, 1, @ProcName, @AuthorUserId, @ExerciseId);

	select @ReviewerCount = count(*) from @ReviewerUserIds;

	if @ReviewerCount > 0 begin
		if exists (
			select *
			from @ReviewerUserIds R
				left join dbo.relLearnersTutors LT on LT.LearnerUserId = @AuthorUserId and R.UserId = LT.TutorUserId
			where LT.TutorUserId is null
		)
			raiserror('%s,%d:: The user has no relationship with a proposed reviewer.', 16, 1, @ProcName, @AuthorUserId);
	end

	select @AuthorName = DisplayName from dbo.appUsers where Id = @AuthorUserId;

	--declare @Now datetime2(0) = sysutcdatetime();
	declare @ReviewId int;

	if @ExternalTran = 0
		begin transaction;

		insert dbo.exeReviews (ExerciseId, Reward, AuthorName)
			values (@ExerciseId, @Reward, @AuthorName);

		select @ReviewId = scope_identity() where @@rowcount != 0;

	    if @ReviewerCount > 0 begin
			insert dbo.exeRequests (ReviewId, Reward, AuthorName, ReviewerUserId, TypeId, [Length])
				select distinct @ReviewId, @Reward, @AuthorName, UserId, @TypeId, @Length 
				from @ReviewerUserIds
		end
		else begin
			insert dbo.exeRequests (ReviewId, Reward, AuthorName, TypeId, [Length])
				values (@ReviewId, @Reward, @AuthorName, @TypeId, @Length);
		end;

		set @Attribute = cast(@ReviewId as nvarchar(100));

		exec dbo.accChangeEscrow @AuthorUserId, @Reward, 'EXRR', @Attribute, null;

	if @ExternalTran = 0
		commit;

	--select @ReviewId as Id, @ExerciseId as ExerciseId, @Reward as Reward, @Now as RequestTime;
	select Id, ExerciseId, Reward, RequestTime from dbo.exeReviews where Id = @ReviewId;

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

