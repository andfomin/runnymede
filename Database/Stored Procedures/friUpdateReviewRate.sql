
CREATE PROCEDURE [dbo].[friUpdateReviewRate]
	@UserId int,
	@FriendUserId int,	
	@ExerciseType char(6),
	@Rate numeric(9, 2)
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if (@Rate < 0)
		raiserror('%s,%d::Review rate must not be negative.', 16, 1, @ProcName, @UserId);

	if (
		(nullif(@Rate, 0) is not null) 
		and (not exists (select * from dbo.appUsers where Id = @UserId and IsTeacher = 1))
	)
		raiserror('%s,%d::To set a non-zero review rate the user must have the teacher status.', 16, 1, @ProcName, @UserId);

	if @ExternalTran = 0
		begin transaction;

		update dbo.friFriends 	
		set 
			RecordingRate = iif(dbo.exeIsTypeRecording(@ExerciseType) = 1, @Rate, RecordingRate),
			WritingRate = iif(dbo.exeIsTypeWriting(@ExerciseType) = 1, @Rate, WritingRate)		
		where UserId = @UserId
			and FriendUserId = @FriendUserId;

		if @@rowcount = 0
			raiserror('%s,%d::Friendship not found.', 16, 1, @ProcName, @UserId);

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
    ON OBJECT::[dbo].[friUpdateReviewRate] TO [websiterole]
    AS [dbo];

